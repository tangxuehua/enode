using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ECommon.Components;
using ECommon.Dapper;
using ECommon.IO;
using ECommon.Logging;
using ECommon.Serializing;
using ECommon.Utilities;
using ENode.Configurations;

namespace ENode.Eventing.Impl
{
    public class SqlServerEventStore : IEventStore
    {
        #region Private Variables

        private readonly string _connectionString;
        private readonly string _tableName;
        private readonly int _tableCount;
        private readonly string _versionIndexName;
        private readonly string _commandIndexName;
        private readonly int _bulkCopyBatchSize;
        private readonly int _bulkCopyTimeout;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IEventSerializer _eventSerializer;
        private readonly IOHelper _ioHelper;
        private readonly ILogger _logger;
        private const string EventTableNameFormat = "{0}_{1}";

        #endregion

        #region Public Properties

        public bool SupportBatchAppendEvent { get; set; }

        #endregion

        #region Constructors

        public SqlServerEventStore(OptionSetting optionSetting)
        {
            if (optionSetting != null)
            {
                _connectionString = optionSetting.GetOptionValue<string>("ConnectionString");
                _tableName = optionSetting.GetOptionValue<string>("TableName");
                _tableCount = optionSetting.GetOptionValue<int>("TableCount");
                _versionIndexName = optionSetting.GetOptionValue<string>("VersionIndexName");
                _commandIndexName = optionSetting.GetOptionValue<string>("CommandIndexName");
                _bulkCopyBatchSize = optionSetting.GetOptionValue<int>("BulkCopyBatchSize");
                _bulkCopyTimeout = optionSetting.GetOptionValue<int>("BulkCopyTimeout");
            }
            else
            {
                var setting = ENodeConfiguration.Instance.Setting.DefaultDBConfigurationSetting;
                _connectionString = setting.ConnectionString;
                _tableName = setting.EventTableName;
                _tableCount = setting.EventTableCount;
                _versionIndexName = setting.EventTableVersionUniqueIndexName;
                _commandIndexName = setting.EventTableCommandIdUniqueIndexName;
                _bulkCopyBatchSize = setting.EventTableBulkCopyBatchSize;
                _bulkCopyTimeout = setting.EventTableBulkCopyTimeout;
            }

            Ensure.NotNull(_connectionString, "_connectionString");
            Ensure.NotNull(_tableName, "_tableName");
            Ensure.NotNull(_versionIndexName, "_versionIndexName");
            Ensure.NotNull(_commandIndexName, "_commandIndexName");
            Ensure.Positive(_bulkCopyBatchSize, "_bulkCopyBatchSize");
            Ensure.Positive(_bulkCopyTimeout, "_bulkCopyTimeout");

            _jsonSerializer = ObjectContainer.Resolve<IJsonSerializer>();
            _eventSerializer = ObjectContainer.Resolve<IEventSerializer>();
            _ioHelper = ObjectContainer.Resolve<IOHelper>();
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().FullName);

            SupportBatchAppendEvent = true;
        }

        #endregion

        #region Public Methods

        public IEnumerable<DomainEventStream> QueryAggregateEvents(string aggregateRootId, string aggregateRootTypeName, int minVersion, int maxVersion)
        {
            var records = _ioHelper.TryIOFunc(() =>
            {
                try
                {
                    using (var connection = GetConnection())
                    {
                        var sql = string.Format("SELECT * FROM [{0}] WHERE AggregateRootId = @AggregateRootId AND Version >= @MinVersion AND Version <= @MaxVersion", GetTableName(aggregateRootId));
                        return connection.Query<StreamRecord>(sql, new
                        {
                            AggregateRootId = aggregateRootId,
                            MinVersion = minVersion,
                            MaxVersion = maxVersion
                        });
                    }
                }
                catch (SqlException ex)
                {
                    var errorMessage = string.Format("Failed to query aggregate events, aggregateRootId: {0}, aggregateRootType: {1}", aggregateRootId, aggregateRootTypeName);
                    _logger.Error(errorMessage, ex);
                    throw new IOException(errorMessage, ex);
                }
                catch (Exception ex)
                {
                    var errorMessage = string.Format("Failed to query aggregate events, aggregateRootId: {0}, aggregateRootType: {1}", aggregateRootId, aggregateRootTypeName);
                    _logger.Error(errorMessage, ex);
                    throw;
                }
            }, "QueryAggregateEvents");

            return records.Select(record => ConvertFrom(record));
        }
        public Task<AsyncTaskResult<IEnumerable<DomainEventStream>>> QueryAggregateEventsAsync(string aggregateRootId, string aggregateRootTypeName, int minVersion, int maxVersion)
        {
            return _ioHelper.TryIOFuncAsync(async () =>
            {
                try
                {
                    using (var connection = GetConnection())
                    {
                        var sql = string.Format("SELECT * FROM [{0}] WHERE AggregateRootId = @AggregateRootId AND Version >= @MinVersion AND Version <= @MaxVersion", GetTableName(aggregateRootId));
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
                    var errorMessage = string.Format("Failed to query aggregate events async, aggregateRootId: {0}, aggregateRootType: {1}", aggregateRootId, aggregateRootTypeName);
                    _logger.Error(errorMessage, ex);
                    return new AsyncTaskResult<IEnumerable<DomainEventStream>>(AsyncTaskStatus.IOException, ex.Message);
                }
                catch (Exception ex)
                {
                    var errorMessage = string.Format("Failed to query aggregate events async, aggregateRootId: {0}, aggregateRootType: {1}", aggregateRootId, aggregateRootTypeName);
                    _logger.Error(errorMessage, ex);
                    return new AsyncTaskResult<IEnumerable<DomainEventStream>>(AsyncTaskStatus.Failed, ex.Message);
                }
            }, "QueryAggregateEventsAsync");
        }
        public Task<AsyncTaskResult<EventAppendResult>> BatchAppendAsync(IEnumerable<DomainEventStream> eventStreams)
        {
            if (eventStreams.Count() == 0)
            {
                throw new ArgumentException("Event streams cannot be empty.");
            }
            var table = BuildEventTable();
            var aggregateRootIds = eventStreams.Select(x => x.AggregateRootId).Distinct();
            if (aggregateRootIds.Count() > 1)
            {
                throw new ArgumentException("Batch append event only support for one aggregate.");
            }
            var aggregateRootId = aggregateRootIds.Single();

            foreach (var eventStream in eventStreams)
            {
                AddDataRow(table, eventStream);
            }

            return _ioHelper.TryIOFuncAsync(async () =>
            {
                try
                {
                    using (var connection = GetConnection())
                    {
                        await connection.OpenAsync();
                        var transaction = await Task.Run<SqlTransaction>(() => connection.BeginTransaction());

                        using (var copy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
                        {
                            InitializeSqlBulkCopy(copy, aggregateRootId);
                            try
                            {
                                await copy.WriteToServerAsync(table);
                                await Task.Run(() => transaction.Commit());
                                return new AsyncTaskResult<EventAppendResult>(AsyncTaskStatus.Success, EventAppendResult.Success);
                            }
                            catch
                            {
                                try
                                {
                                    transaction.Rollback();
                                }
                                catch (Exception ex)
                                {
                                    _logger.ErrorFormat("Transaction rollback failed.", ex);
                                }
                                throw;
                            }
                        }
                    }
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2601 && ex.Message.Contains(_versionIndexName))
                    {
                        return new AsyncTaskResult<EventAppendResult>(AsyncTaskStatus.Success, EventAppendResult.DuplicateEvent);
                    }
                    else if (ex.Number == 2601 && ex.Message.Contains(_commandIndexName))
                    {
                        return new AsyncTaskResult<EventAppendResult>(AsyncTaskStatus.Success, EventAppendResult.DuplicateCommand);
                    }
                    _logger.Error("Batch append event has sql exception.", ex);
                    return new AsyncTaskResult<EventAppendResult>(AsyncTaskStatus.IOException, ex.Message, EventAppendResult.Failed);
                }
                catch (Exception ex)
                {
                    _logger.Error("Batch append event has unknown exception.", ex);
                    return new AsyncTaskResult<EventAppendResult>(AsyncTaskStatus.Failed, ex.Message, EventAppendResult.Failed);
                }
            }, "BatchAppendEventsAsync");
        }
        public Task<AsyncTaskResult<EventAppendResult>> AppendAsync(DomainEventStream eventStream)
        {
            var record = ConvertTo(eventStream);

            return _ioHelper.TryIOFuncAsync(async () =>
            {
                try
                {
                    using (var connection = GetConnection())
                    {
                        await connection.InsertAsync(record, GetTableName(record.AggregateRootId));
                        return new AsyncTaskResult<EventAppendResult>(AsyncTaskStatus.Success, EventAppendResult.Success);
                    }
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2601 && ex.Message.Contains(_versionIndexName))
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
                        var result = await connection.QueryListAsync<StreamRecord>(new { AggregateRootId = aggregateRootId, Version = version }, GetTableName(aggregateRootId));
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
                        var result = await connection.QueryListAsync<StreamRecord>(new { AggregateRootId = aggregateRootId, CommandId = commandId }, GetTableName(aggregateRootId));
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

        #endregion

        #region Private Methods

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
                return _tableName;
            }

            var tableIndex = GetTableIndex(aggregateRootId);

            return string.Format(EventTableNameFormat, _tableName, tableIndex);
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
                record.AggregateRootTypeName,
                record.Version,
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
        private DataTable BuildEventTable()
        {
            var table = new DataTable();
            table.Columns.Add("AggregateRootId", typeof(string));
            table.Columns.Add("AggregateRootTypeName", typeof(string));
            table.Columns.Add("Version", typeof(int));
            table.Columns.Add("CommandId", typeof(string));
            table.Columns.Add("CreatedOn", typeof(DateTime));
            table.Columns.Add("Events", typeof(string));
            return table;
        }
        private void AddDataRow(DataTable table, DomainEventStream eventStream)
        {
            var row = table.NewRow();
            row["AggregateRootId"] = eventStream.AggregateRootId;
            row["AggregateRootTypeName"] = eventStream.AggregateRootTypeName;
            row["CommandId"] = eventStream.CommandId;
            row["Version"] = eventStream.Version;
            row["CreatedOn"] = eventStream.Timestamp;
            row["Events"] = _jsonSerializer.Serialize(_eventSerializer.Serialize(eventStream.Events));
            table.Rows.Add(row);
        }
        private void InitializeSqlBulkCopy(SqlBulkCopy copy, string aggregateRootId)
        {
            copy.BatchSize = _bulkCopyBatchSize;
            copy.BulkCopyTimeout = _bulkCopyTimeout;
            copy.DestinationTableName = GetTableName(aggregateRootId);
            copy.ColumnMappings.Add("AggregateRootId", "AggregateRootId");
            copy.ColumnMappings.Add("AggregateRootTypeName", "AggregateRootTypeName");
            copy.ColumnMappings.Add("CommandId", "CommandId");
            copy.ColumnMappings.Add("Version", "Version");
            copy.ColumnMappings.Add("CreatedOn", "CreatedOn");
            copy.ColumnMappings.Add("Events", "Events");
        }

        #endregion

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
