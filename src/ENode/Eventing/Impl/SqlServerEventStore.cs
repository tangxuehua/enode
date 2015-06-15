using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using ECommon.Components;
using ECommon.Dapper;
using ECommon.IO;
using ECommon.Logging;
using ECommon.Serializing;
using ECommon.Utilities;
using ENode.Configurations;
using ENode.Infrastructure;

namespace ENode.Eventing.Impl
{
    public class SqlServerEventStore : IEventStore
    {
        #region Private Variables

        private readonly string _connectionString;
        private readonly string _eventTable;
        private readonly string _primaryKeyName;
        private readonly string _commandIndexName;
        private readonly int _bulkCopyBatchSize;
        private readonly int _bulkCopyTimeout;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IEventSerializer _eventSerializer;
        private readonly IOHelper _ioHelper;
        private readonly ILogger _logger;

        #endregion

        #region Constructors

        public SqlServerEventStore()
        {
            var configSetting = ENodeConfiguration.Instance.Setting;
            var setting = configSetting.SqlServerEventStoreSetting;
            Ensure.NotNull(setting, "SqlServerEventStoreSetting");
            Ensure.NotNull(setting.ConnectionString, "SqlServerEventStoreSetting.ConnectionString");
            Ensure.NotNull(setting.TableName, "SqlServerEventStoreSetting.TableName");
            Ensure.NotNull(setting.PrimaryKeyName, "SqlServerEventStoreSetting.PrimaryKeyName");
            Ensure.Positive(configSetting.SqlServerBulkCopyBatchSize, "SqlServerBulkCopyBatchSize");
            Ensure.Positive(configSetting.SqlServerBulkCopyTimeout, "SqlServerBulkCopyTimeout");

            _connectionString = setting.ConnectionString;
            _eventTable = setting.TableName;
            _primaryKeyName = setting.PrimaryKeyName;
            _commandIndexName = setting.CommandIndexName;
            _bulkCopyBatchSize = configSetting.SqlServerBulkCopyBatchSize;
            _bulkCopyTimeout = configSetting.SqlServerBulkCopyTimeout;
            _jsonSerializer = ObjectContainer.Resolve<IJsonSerializer>();
            _eventSerializer = ObjectContainer.Resolve<IEventSerializer>();
            _ioHelper = ObjectContainer.Resolve<IOHelper>();
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().FullName);
        }

        #endregion

        #region Public Methods

        public bool SupportBatchAppend
        {
            get { return true; }
        }
        public IEnumerable<DomainEventStream> QueryAggregateEvents(string aggregateRootId, int aggregateRootTypeCode, int minVersion, int maxVersion)
        {
            var records = _ioHelper.TryIOFunc(() =>
            {
                using (var connection = GetConnection())
                {
                    var sql = string.Format("SELECT * FROM [{0}] WHERE AggregateRootId = @AggregateRootId AND Version >= @MinVersion AND Version <= @MaxVersion", _eventTable);
                    return connection.Query<StreamRecord>(sql, new
                    {
                        AggregateRootId = aggregateRootId,
                        MinVersion = minVersion,
                        MaxVersion = maxVersion
                    });
                }
            }, "QueryAggregateEvents");

            return records.Select(record => ConvertFrom(record));
        }
        public IEnumerable<DomainEventStream> QueryByPage(int pageIndex, int pageSize)
        {
            var records = _ioHelper.TryIOFunc(() =>
            {
                using (var connection = GetConnection())
                {
                    return connection.QueryPaged<StreamRecord>(null, _eventTable, "Sequence", pageIndex, pageSize);
                }
            }, "QueryByPage");

            return records.Select(record => ConvertFrom(record));
        }

        public Task<AsyncTaskResult> BatchAppendAsync(IEnumerable<DomainEventStream> eventStreams)
        {
            var table = BuildEventTable();
            foreach (var eventStream in eventStreams)
            {
                AddDataRow(table, eventStream);
            }

            return _ioHelper.TryIOFuncAsync<AsyncTaskResult>(async () =>
            {
                try
                {
                    using (var connection = GetConnection())
                    {
                        await connection.OpenAsync();
                        var transaction = await Task.Run<SqlTransaction>(() => connection.BeginTransaction());

                        using (var copy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
                        {
                            copy.BatchSize = _bulkCopyBatchSize;
                            copy.BulkCopyTimeout = _bulkCopyTimeout;
                            copy.DestinationTableName = _eventTable;
                            copy.ColumnMappings.Add("CommandId", "CommandId");
                            copy.ColumnMappings.Add("AggregateRootId", "AggregateRootId");
                            copy.ColumnMappings.Add("AggregateRootTypeCode", "AggregateRootTypeCode");
                            copy.ColumnMappings.Add("Version", "Version");
                            copy.ColumnMappings.Add("Timestamp", "Timestamp");
                            copy.ColumnMappings.Add("Events", "Events");

                            try
                            {
                                await copy.WriteToServerAsync(table);
                                await Task.Run(() => transaction.Commit());
                                return AsyncTaskResult.Success;
                            }
                            catch (Exception ex)
                            {
                                try { transaction.Rollback(); }
                                catch { }
                                return new AsyncTaskResult(AsyncTaskStatus.Failed, ex.Message);
                            }
                        }
                    }
                }
                catch (SqlException ex)
                {
                    _logger.Error("Batch append event has sql exception.", ex);
                    return new AsyncTaskResult(AsyncTaskStatus.IOException, ex.Message);
                }
                catch (Exception ex)
                {
                    _logger.Error("Batch append event has unknown exception.", ex);
                    return new AsyncTaskResult(AsyncTaskStatus.Failed, ex.Message);
                }
            }, "BatchAppendEventsAsync");
        }
        public Task<AsyncTaskResult<EventAppendResult>> AppendAsync(DomainEventStream eventStream)
        {
            var record = ConvertTo(eventStream);

            return _ioHelper.TryIOFuncAsync<AsyncTaskResult<EventAppendResult>>(async () =>
            {
                try
                {
                    using (var connection = GetConnection())
                    {
                        await connection.InsertAsync(record, _eventTable);
                        return new AsyncTaskResult<EventAppendResult>(AsyncTaskStatus.Success, EventAppendResult.Success);
                    }
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627 && ex.Message.Contains(_primaryKeyName))
                    {
                        return new AsyncTaskResult<EventAppendResult>(AsyncTaskStatus.Success, EventAppendResult.DuplicateEvent);
                    }
                    else if (ex.Number == 2601 && ex.Message.Contains(_commandIndexName))
                    {
                        return new AsyncTaskResult<EventAppendResult>(AsyncTaskStatus.Success, EventAppendResult.DuplicateCommand);
                    }
                    _logger.Error(string.Format("Append event has sql exception, eventStream: {0}", eventStream), ex);
                    return new AsyncTaskResult<EventAppendResult>(AsyncTaskStatus.IOException, ex.Message, EventAppendResult.Failed);
                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("Append event has unknown exception, eventStream: {0}", eventStream), ex);
                    return new AsyncTaskResult<EventAppendResult>(AsyncTaskStatus.Failed, ex.Message, EventAppendResult.Failed);
                }
            }, "AppendEventsAsync");
        }
        public Task<AsyncTaskResult<DomainEventStream>> FindAsync(string aggregateRootId, int version)
        {
            return _ioHelper.TryIOFuncAsync<AsyncTaskResult<DomainEventStream>>(async () =>
            {
                try
                {
                    using (var connection = GetConnection())
                    {
                        var result = await connection.QueryListAsync<StreamRecord>(new { AggregateRootId = aggregateRootId, Version = version }, _eventTable);
                        var record = result.SingleOrDefault();
                        var stream = record != null ? ConvertFrom(record) : null;
                        return new AsyncTaskResult<DomainEventStream>(AsyncTaskStatus.Success, stream);
                    }
                }
                catch (SqlException ex)
                {
                    _logger.Error(string.Format("Find event by version has sql exception, aggregateRootId: {0}, version: {1}", aggregateRootId, version), ex);
                    return new AsyncTaskResult<DomainEventStream>(AsyncTaskStatus.IOException, ex.Message);
                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("Find event by version has unknown exception, aggregateRootId: {0}, version: {1}", aggregateRootId, version), ex);
                    return new AsyncTaskResult<DomainEventStream>(AsyncTaskStatus.Failed, ex.Message);
                }
            }, "FindEventByVersionAsync");
        }
        public Task<AsyncTaskResult<DomainEventStream>> FindAsync(string aggregateRootId, string commandId)
        {
            return _ioHelper.TryIOFuncAsync<AsyncTaskResult<DomainEventStream>>(async () =>
            {
                try
                {
                    using (var connection = GetConnection())
                    {
                        var result = await connection.QueryListAsync<StreamRecord>(new { AggregateRootId = aggregateRootId, CommandId = commandId }, _eventTable);
                        var record = result.SingleOrDefault();
                        var stream = record != null ? ConvertFrom(record) : null;
                        return new AsyncTaskResult<DomainEventStream>(AsyncTaskStatus.Success, stream);
                    }
                }
                catch (SqlException ex)
                {
                    _logger.Error(string.Format("Find event by commandId has sql exception, aggregateRootId: {0}, commandId: {1}", aggregateRootId, commandId), ex);
                    return new AsyncTaskResult<DomainEventStream>(AsyncTaskStatus.IOException, ex.Message);
                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("Find event by commandId has unknown exception, aggregateRootId: {0}, commandId: {1}", aggregateRootId, commandId), ex);
                    return new AsyncTaskResult<DomainEventStream>(AsyncTaskStatus.Failed, ex.Message);
                }
            }, "FindEventByCommandIdAsync");
        }
        public Task<AsyncTaskResult<IEnumerable<DomainEventStream>>> QueryAggregateEventsAsync(string aggregateRootId, int aggregateRootTypeCode, int minVersion, int maxVersion)
        {
            return _ioHelper.TryIOFuncAsync<AsyncTaskResult<IEnumerable<DomainEventStream>>>(async () =>
            {
                try
                {
                    using (var connection = GetConnection())
                    {
                        var sql = string.Format("SELECT * FROM [{0}] WHERE AggregateRootId = @AggregateRootId AND Version >= @MinVersion AND Version <= @MaxVersion", _eventTable);
                        var result = await connection.QueryAsync<StreamRecord>(sql, new
                        {
                            AggregateRootId = aggregateRootId,
                            MinVersion = minVersion,
                            MaxVersion = maxVersion
                        });
                        var streams = result.Select(record => ConvertFrom(record));
                        return new AsyncTaskResult<IEnumerable<DomainEventStream>>(AsyncTaskStatus.Success, streams);
                    }
                }
                catch (SqlException ex)
                {
                    _logger.Error(string.Format("Query aggregate events has sql exception, aggregateRootId: {0}", aggregateRootId), ex);
                    return new AsyncTaskResult<IEnumerable<DomainEventStream>>(AsyncTaskStatus.IOException, ex.Message);
                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("Query aggregate events has unknown exception, aggregateRootId: {0}", aggregateRootId), ex);
                    return new AsyncTaskResult<IEnumerable<DomainEventStream>>(AsyncTaskStatus.Failed, ex.Message);
                }
            }, "QueryAggregateEventsAsync");
        }
        public Task<AsyncTaskResult<IEnumerable<DomainEventStream>>> QueryByPageAsync(int pageIndex, int pageSize)
        {
            return _ioHelper.TryIOFuncAsync<AsyncTaskResult<IEnumerable<DomainEventStream>>>(async () =>
            {
                try
                {
                    using (var connection = GetConnection())
                    {
                        var result = await connection.QueryPagedAsync<StreamRecord>(null, _eventTable, "Sequence", pageIndex, pageSize);
                        var streams = result.Select(record => ConvertFrom(record));
                        return new AsyncTaskResult<IEnumerable<DomainEventStream>>(AsyncTaskStatus.Success, streams);
                    }
                }
                catch (SqlException ex)
                {
                    _logger.Error(string.Format("Query events by page has sql exception, pageIndex: {0}, pageSize: {1}", pageIndex, pageSize), ex);
                    return new AsyncTaskResult<IEnumerable<DomainEventStream>>(AsyncTaskStatus.IOException, ex.Message);
                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("Query events by page has unknown exception, pageIndex: {0}, pageSize: {1}", pageIndex, pageSize), ex);
                    return new AsyncTaskResult<IEnumerable<DomainEventStream>>(AsyncTaskStatus.Failed, ex.Message);
                }
            }, "QueryByPageAsync");
        }

        #endregion

        #region Private Methods

        private DataTable BuildEventTable()
        {
            var table = new DataTable();
            table.Columns.Add("CommandId", typeof(string));
            table.Columns.Add("AggregateRootId", typeof(string));
            table.Columns.Add("AggregateRootTypeCode", typeof(int));
            table.Columns.Add("Version", typeof(int));
            table.Columns.Add("Timestamp", typeof(DateTime));
            table.Columns.Add("Events", typeof(string));
            return table;
        }
        private SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
        private DomainEventStream ConvertFrom(StreamRecord record)
        {
            return new DomainEventStream(
                record.CommandId,
                record.AggregateRootId,
                record.AggregateRootTypeCode,
                record.Version,
                record.Timestamp,
                _eventSerializer.Deserialize<IDomainEvent>(_jsonSerializer.Deserialize<IDictionary<int, string>>(record.Events)));
        }
        private StreamRecord ConvertTo(DomainEventStream eventStream)
        {
            return new StreamRecord
            {
                CommandId = eventStream.CommandId,
                AggregateRootId = eventStream.AggregateRootId,
                AggregateRootTypeCode = eventStream.AggregateRootTypeCode,
                Version = eventStream.Version,
                Timestamp = eventStream.Timestamp,
                Events = _jsonSerializer.Serialize(_eventSerializer.Serialize(eventStream.Events))
            };
        }
        private void AddDataRow(DataTable table, DomainEventStream eventStream)
        {
            var row = table.NewRow();
            row["CommandId"] = eventStream.CommandId;
            row["AggregateRootId"] = eventStream.AggregateRootId;
            row["AggregateRootTypeCode"] = eventStream.AggregateRootTypeCode;
            row["Version"] = eventStream.Version;
            row["Timestamp"] = eventStream.Timestamp;
            row["Events"] = _jsonSerializer.Serialize(_eventSerializer.Serialize(eventStream.Events));
            table.Rows.Add(row);
        }

        #endregion

        class StreamRecord
        {
            public int AggregateRootTypeCode { get; set; }
            public string AggregateRootId { get; set; }
            public int Version { get; set; }
            public string CommandId { get; set; }
            public DateTime Timestamp { get; set; }
            public string Events { get; set; }
        }
    }
}
