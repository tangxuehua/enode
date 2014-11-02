using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using ECommon.Components;
using ECommon.Dapper;
using ECommon.Serializing;
using ECommon.Utilities;
using ENode.Configurations;
using ENode.Infrastructure;

namespace ENode.Eventing.Impl.SQL
{
    public class SqlServerEventStore : IEventStore
    {
        #region Private Variables

        private readonly string _connectionString;
        private readonly string _eventTable;
        private readonly string _primaryKeyName;
        private readonly IBinarySerializer _binarySerializer;

        #endregion

        #region Constructors

        public SqlServerEventStore()
        {
            var setting = ENodeConfiguration.Instance.Setting.SqlServerEventStoreSetting;
            Ensure.NotNull(setting, "SqlServerEventStoreSetting");
            Ensure.NotNull(setting.ConnectionString, "SqlServerEventStoreSetting.ConnectionString");
            Ensure.NotNull(setting.TableName, "SqlServerEventStoreSetting.TableName");
            Ensure.NotNull(setting.PrimaryKeyName, "SqlServerEventStoreSetting.PrimaryKeyName");

            _connectionString = setting.ConnectionString;
            _eventTable = setting.TableName;
            _primaryKeyName = setting.PrimaryKeyName;
            _binarySerializer = ObjectContainer.Resolve<IBinarySerializer>();
        }

        #endregion

        #region Public Methods

        public EventAppendResult BatchAppend(IEnumerable<DomainEventStream> eventStreams)
        {
            var appendResult = EventAppendResult.Success;
            using (var connection = GetConnection())
            {
                connection.Open();
                var transaction = connection.BeginTransaction();
                try
                {
                    foreach (var eventStream in eventStreams)
                    {
                        var record = ConvertTo(eventStream);
                        try
                        {
                            connection.Insert(record, _eventTable);
                        }
                        catch (SqlException ex)
                        {
                            if (ex.Number == 2627 && ex.Message.Contains(_primaryKeyName))
                            {
                                appendResult = EventAppendResult.DuplicateEvent;
                                break;
                            }
                            throw;
                        }
                    }
                    if (appendResult == EventAppendResult.Success)
                    {
                        transaction.Commit();
                    }
                    else
                    {
                        transaction.Rollback();
                    }
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            return appendResult;
        }
        public EventAppendResult Append(DomainEventStream eventStream)
        {
            var record = ConvertTo(eventStream);

            using (var connection = GetConnection())
            {
                connection.Open();
                try
                {
                    connection.Insert(record, _eventTable);
                    return EventAppendResult.Success;
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627)
                    {
                        if (ex.Message.Contains(_primaryKeyName))
                        {
                            return EventAppendResult.DuplicateEvent;
                        }
                    }
                    throw;
                }
            }
        }
        public DomainEventStream Find(string aggregateRootId, int version)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                var record = connection.QueryList<StreamRecord>(new { AggregateRootId = aggregateRootId, Version = version }, _eventTable).SingleOrDefault();
                if (record != null)
                {
                    return ConvertFrom(record);
                }
                return null;
            }
        }
        public DomainEventStream Find(string aggregateRootId, string commandId)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                var record = connection.QueryList<StreamRecord>(new { AggregateRootId = aggregateRootId, CommandId = commandId }, _eventTable).SingleOrDefault();
                if (record != null)
                {
                    return ConvertFrom(record);
                }
                return null;
            }
        }
        public IEnumerable<DomainEventStream> QueryAggregateEvents(string aggregateRootId, int aggregateRootTypeCode, int minVersion, int maxVersion)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    var sql = string.Format("SELECT * FROM [{0}] WHERE AggregateRootId = @AggregateRootId AND Version >= @MinVersion AND Version <= @MaxVersion", _eventTable);
                    var records = connection.Query<StreamRecord>(sql,
                    new
                    {
                        AggregateRootId = aggregateRootId,
                        MinVersion = minVersion,
                        MaxVersion = maxVersion
                    });
                    var streams = new List<DomainEventStream>();
                    foreach (var record in records)
                    {
                        streams.Add(ConvertFrom(record));
                    }
                    return streams;
                }
            }
            catch (Exception ex)
            {
                throw new IOException("Query events from sql server has exception, aggregateRootTypeCode:{0}, aggregateRootId:{1}", ex, aggregateRootTypeCode, aggregateRootId);
            }
        }
        public IEnumerable<DomainEventStream> QueryByPage(int pageIndex, int pageSize)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                var records = connection.QueryPaged<StreamRecord>(null, _eventTable, "Sequence", pageIndex, pageSize);
                var streams = new List<DomainEventStream>();
                foreach (var record in records)
                {
                    streams.Add(ConvertFrom(record));
                }
                return streams;
            }
        }

        #endregion

        #region Private Methods

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
                _binarySerializer.Deserialize<IEnumerable<IDomainEvent>>(record.Events),
                _binarySerializer.Deserialize<IDictionary<string, string>>(record.Items));
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
                Events = _binarySerializer.Serialize(eventStream.Events),
                Items = _binarySerializer.Serialize(eventStream.Items)
            };
        }

        #endregion

        class StreamRecord
        {
            public int AggregateRootTypeCode { get; set; }
            public string AggregateRootId { get; set; }
            public int Version { get; set; }
            public string CommandId { get; set; }
            public DateTime Timestamp { get; set; }
            public byte[] Events { get; set; }
            public byte[] Items { get; set; }
        }
    }
}
