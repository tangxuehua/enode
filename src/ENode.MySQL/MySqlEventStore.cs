using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ECommon.Components;
using ECommon.Dapper;
using ECommon.IO;
using ECommon.Logging;
using ECommon.Serializing;
using ECommon.Utilities;
using ENode.Eventing;
using MySql.Data.MySqlClient;

namespace ENode.MySQL
{
    public class MySqlEventStore : IEventStore
    {
        private const string EventSingleTableNameFormat = "`{0}`";
        private const string EventTableNameFormat = "`{0}_{1}`";
        private const string QueryEventsSql = "SELECT * FROM {0} WHERE AggregateRootId = @AggregateRootId AND Version >= @MinVersion AND Version <= @MaxVersion ORDER BY Version ASC";
        private const string InsertEventSql = "INSERT INTO {0} (AggregateRootId, AggregateRootTypeName, CommandId, Version, CreatedOn, Events) VALUES (@AggregateRootId, @AggregateRootTypeName, @CommandId, @Version, @CreatedOn, @Events)";

        #region Private Variables

        private string _connectionString;
        private string _tableName;
        private int _tableCount;
        private string _versionIndexName;
        private string _commandIndexName;
        private int _batchInsertTimeoutSeconds;
        private IJsonSerializer _jsonSerializer;
        private IEventSerializer _eventSerializer;
        private IOHelper _ioHelper;
        private ILogger _logger;

        #endregion

        #region Public Methods

        public MySqlEventStore Initialize(
            string connectionString,
            string tableName = "EventStream",
            int tableCount = 1,
            string versionIndexName = "IX_EventStream_AggId_Version",
            string commandIndexName = "IX_EventStream_AggId_CommandId",
            int batchInsertTimeoutSeconds = 60)
        {
            _connectionString = connectionString;
            _tableName = tableName;
            _tableCount = tableCount;
            _versionIndexName = versionIndexName;
            _commandIndexName = commandIndexName;
            _batchInsertTimeoutSeconds = batchInsertTimeoutSeconds;

            Ensure.NotNull(_connectionString, "_connectionString");
            Ensure.NotNull(_tableName, "_tableName");
            Ensure.Positive(_tableCount, "_tableCount");
            Ensure.NotNull(_versionIndexName, "_versionIndexName");
            Ensure.NotNull(_commandIndexName, "_commandIndexName");
            Ensure.Positive(_batchInsertTimeoutSeconds, "_batchInsertTimeoutSeconds");

            _jsonSerializer = ObjectContainer.Resolve<IJsonSerializer>();
            _eventSerializer = ObjectContainer.Resolve<IEventSerializer>();
            _ioHelper = ObjectContainer.Resolve<IOHelper>();
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().FullName);

            return this;
        }
        public Task<IEnumerable<DomainEventStream>> QueryAggregateEventsAsync(string aggregateRootId, string aggregateRootTypeName, int minVersion, int maxVersion)
        {
            var sql = string.Format(QueryEventsSql, GetTableName(aggregateRootId));

            return _ioHelper.TryIOFuncAsync(async () =>
            {
                try
                {
                    using (var connection = GetConnection())
                    {
                        await connection.OpenAsync().ConfigureAwait(false);
                        var result = await connection.QueryAsync<StreamRecord>(sql, new
                        {
                            AggregateRootId = aggregateRootId,
                            MinVersion = minVersion,
                            MaxVersion = maxVersion
                        }).ConfigureAwait(false);
                        return result.Select(ConvertFrom);
                    }
                }
                catch (Exception ex)
                {
                    var errorMessage = string.Format("Failed to query aggregate events async, aggregateRootId: {0}, aggregateRootType: {1}", aggregateRootId, aggregateRootTypeName);
                    _logger.Error(errorMessage, ex);
                    throw;
                }
            }, "QueryAggregateEventsAsync");
        }
        public Task<EventAppendResult> BatchAppendAsync(IEnumerable<DomainEventStream> eventStreams)
        {
            if (eventStreams.Count() == 0)
            {
                return Task.FromResult(new EventAppendResult());
            }

            var eventStreamDict = new Dictionary<string, IList<DomainEventStream>>();
            var aggregateRootIdList = eventStreams.Select(x => x.AggregateRootId).Distinct().ToList();
            foreach (var aggregateRootId in aggregateRootIdList)
            {
                var eventStreamList = eventStreams.Where(x => x.AggregateRootId == aggregateRootId).ToList();
                if (eventStreamList.Count > 0)
                {
                    eventStreamDict.Add(aggregateRootId, eventStreamList);
                }
            }

            var batchAggregateEventAppendResult = new BatchAggregateEventAppendResult(eventStreamDict.Keys.Count);
            foreach (var entry in eventStreamDict)
            {
                BatchAppendAggregateEventsAsync(entry.Key, entry.Value, batchAggregateEventAppendResult, 0);
            }

            return batchAggregateEventAppendResult.TaskCompletionSource.Task;  
        }
        public Task<DomainEventStream> FindAsync(string aggregateRootId, int version)
        {
            return _ioHelper.TryIOFuncAsync(async () =>
            {
                try
                {
                    using (var connection = GetConnection())
                    {
                        await connection.OpenAsync().ConfigureAwait(false);
                        var result = await connection.QueryListAsync<StreamRecord>(new { AggregateRootId = aggregateRootId, Version = version }, GetTableName(aggregateRootId)).ConfigureAwait(false);
                        var record = result.SingleOrDefault();
                        return record != null ? ConvertFrom(record) : null;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("Find event by version has unknown exception, aggregateRootId: {0}, version: {1}", aggregateRootId, version), ex);
                    throw;
                }
            }, "FindEventByVersionAsync");
        }
        public Task<DomainEventStream> FindAsync(string aggregateRootId, string commandId)
        {
            return _ioHelper.TryIOFuncAsync(async () =>
            {
                try
                {
                    using (var connection = GetConnection())
                    {
                        await connection.OpenAsync().ConfigureAwait(false);
                        var result = await connection.QueryListAsync<StreamRecord>(new { AggregateRootId = aggregateRootId, CommandId = commandId }, GetTableName(aggregateRootId)).ConfigureAwait(false);
                        var record = result.SingleOrDefault();
                        return record != null ? ConvertFrom(record) : null;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("Find event by commandId has unknown exception, aggregateRootId: {0}, commandId: {1}", aggregateRootId, commandId), ex);
                    throw;
                }
            }, "FindEventByCommandIdAsync");
        }

        #endregion

        #region Private Methods

        private void BatchAppendAggregateEventsAsync(string aggregateRootId, IList<DomainEventStream> eventStreamList, BatchAggregateEventAppendResult batchAggregateEventAppendResult, int retryTimes)
        {
            _ioHelper.TryAsyncActionRecursively("BatchAppendAggregateEventsAsync",
            () => BatchAppendAggregateEventsAsync(aggregateRootId, eventStreamList),
            currentRetryTimes => BatchAppendAggregateEventsAsync(aggregateRootId, eventStreamList, batchAggregateEventAppendResult, currentRetryTimes),
            async result =>
            {
                if (result == EventAppendStatus.Success)
                {
                    batchAggregateEventAppendResult.AddCompleteAggregate(aggregateRootId, new AggregateEventAppendResult
                    {
                        EventAppendStatus = EventAppendStatus.Success
                    });
                }
                else if (result == EventAppendStatus.DuplicateEvent)
                {
                    batchAggregateEventAppendResult.AddCompleteAggregate(aggregateRootId, new AggregateEventAppendResult
                    {
                        EventAppendStatus = EventAppendStatus.DuplicateEvent
                    });
                }
                else if (result == EventAppendStatus.DuplicateCommand)
                {
                    var duplicateCommandIds = new List<string>();
                    foreach (var eventStream in eventStreamList)
                    {
                        await TryFindEventByCommandIdAsync(aggregateRootId, eventStream.CommandId, duplicateCommandIds, 0);
                    }
                    batchAggregateEventAppendResult.AddCompleteAggregate(aggregateRootId, new AggregateEventAppendResult
                    {
                        EventAppendStatus = EventAppendStatus.DuplicateCommand,
                        DuplicateCommandIds = duplicateCommandIds
                    });
                }
            },
            () => string.Format("[aggregateRootId: {0}, eventStreamCount: {1}]", aggregateRootId, eventStreamList.Count),
            null,
            retryTimes, true);
        }
        private async Task<EventAppendStatus> BatchAppendAggregateEventsAsync(string aggregateRootId, IList<DomainEventStream> eventStreamList)
        {
            try
            {
                string sql = string.Format(InsertEventSql, GetTableName(aggregateRootId));
                var streamRecords = eventStreamList.Select(ConvertTo);
                using (var connection = GetConnection())
                {
                    await connection.OpenAsync().ConfigureAwait(false);
                    var transaction = await Task.Run(() => connection.BeginTransaction()).ConfigureAwait(false);
                    try
                    {
                        await connection.ExecuteAsync(sql, streamRecords, transaction: transaction, commandTimeout: _batchInsertTimeoutSeconds).ConfigureAwait(false);
                        await Task.Run(() => transaction.Commit()).ConfigureAwait(false);
                        return EventAppendStatus.Success;
                    }
                    catch
                    {
                        try
                        {
                            transaction.Rollback();
                        }
                        catch (Exception ex)
                        {
                            _logger.ErrorFormat("BatchAppendAggregateEvents transaction rollback failed, aggregateRootId:" + aggregateRootId, ex);
                        }
                        throw;
                    }
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062 && ex.Message.Contains(_versionIndexName))
                {
                    return EventAppendStatus.DuplicateEvent;
                }
                if (ex.Number == 1062 && ex.Message.Contains(_commandIndexName))
                {
                    return EventAppendStatus.DuplicateCommand;
                }
                throw;
            }
        }
        private Task TryFindEventByCommandIdAsync(string aggregateRootId, string commandId, IList<string> duplicateCommandIds, int retryTimes)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();

            _ioHelper.TryAsyncActionRecursively("TryFindEventByCommandIdAsync",
            () => FindAsync(aggregateRootId, commandId),
            currentRetryTimes => TryFindEventByCommandIdAsync(aggregateRootId, commandId, duplicateCommandIds, currentRetryTimes),
            result =>
            {
                if (result != null)
                {
                    duplicateCommandIds.Add(commandId);
                }
                taskCompletionSource.SetResult(true);
            },
            () => string.Format("[aggregateRootId:{0}, commandId:{1}]", aggregateRootId, commandId),
            null,
            retryTimes, true);

            return taskCompletionSource.Task;
        }
        private int GetTableIndex(string aggregateRootId)
        {
            int hash = 23;
            foreach (char c in aggregateRootId)
            {
                hash = (hash << 5) - hash + c;
            }
            if (hash < 0)
            {
                hash = Math.Abs(hash);
            }
            return hash % _tableCount;
        }
        private string GetTableName(string aggregateRootId)
        {
            if (_tableCount <= 1)
            {
                return string.Format(EventSingleTableNameFormat, _tableName);
            }

            var tableIndex = GetTableIndex(aggregateRootId);

            return string.Format(EventTableNameFormat, _tableName, tableIndex);
        }
        private MySqlConnection GetConnection()
        {
            return new MySqlConnection(_connectionString);
        }
        private DomainEventStream ConvertFrom(StreamRecord record)
        {
            return new DomainEventStream(
                record.CommandId,
                record.AggregateRootId,
                record.AggregateRootTypeName,
                record.CreatedOn,
                _eventSerializer.Deserialize<IDomainEvent>(_jsonSerializer.Deserialize<IDictionary<string, string>>(record.Events)));
        }
        private StreamRecord ConvertTo(DomainEventStream eventStream)
        {
            return new StreamRecord
            {
                CommandId = eventStream.CommandId,
                AggregateRootId = eventStream.AggregateRootId,
                AggregateRootTypeName = eventStream.AggregateRootTypeName,
                Version = eventStream.Version,
                CreatedOn = eventStream.Timestamp,
                Events = _jsonSerializer.Serialize(_eventSerializer.Serialize(eventStream.Events))
            };
        }

        #endregion

        class BatchAggregateEventAppendResult
        {
            private ConcurrentDictionary<string, AggregateEventAppendResult> _aggregateEventAppendResultDict = new ConcurrentDictionary<string, AggregateEventAppendResult>();
            public TaskCompletionSource<EventAppendResult> TaskCompletionSource = new TaskCompletionSource<EventAppendResult>();
            private readonly int _expectedAggregateRootCount;

            public BatchAggregateEventAppendResult(int expectedAggregateRootCount)
            {
                _expectedAggregateRootCount = expectedAggregateRootCount;
            }

            public void AddCompleteAggregate(string aggregateRootId, AggregateEventAppendResult result)
            {
                if (_aggregateEventAppendResultDict.TryAdd(aggregateRootId, result))
                {
                    var completedAggregateRootCount = _aggregateEventAppendResultDict.Keys.Count;
                    if (completedAggregateRootCount == _expectedAggregateRootCount)
                    {
                        var eventAppendResult = new EventAppendResult();
                        foreach (var entry in _aggregateEventAppendResultDict)
                        {
                            if (entry.Value.EventAppendStatus == EventAppendStatus.Success)
                            {
                                eventAppendResult.AddSuccessAggregateRootId(entry.Key);
                            }
                            else if (entry.Value.EventAppendStatus == EventAppendStatus.DuplicateEvent)
                            {
                                eventAppendResult.AddDuplicateEventAggregateRootId(entry.Key);
                            }
                            else if (entry.Value.EventAppendStatus == EventAppendStatus.DuplicateCommand)
                            {
                                eventAppendResult.AddDuplicateCommandIds(entry.Key, entry.Value.DuplicateCommandIds);
                            }
                        }
                        TaskCompletionSource.TrySetResult(eventAppendResult);
                    }
                }
            }
        }
        class AggregateEventAppendResult
        {
            public EventAppendStatus EventAppendStatus;
            public IList<string> DuplicateCommandIds;
        }
        class StreamRecord
        {
            public string AggregateRootTypeName { get; set; }
            public string AggregateRootId { get; set; }
            public int Version { get; set; }
            public string CommandId { get; set; }
            public DateTime CreatedOn { get; set; }
            public string Events { get; set; }
        }
    }
}
