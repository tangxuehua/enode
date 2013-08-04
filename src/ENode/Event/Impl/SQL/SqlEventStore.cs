using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using ENode.Domain;
using ENode.Infrastructure;

namespace ENode.Eventing.Storage.Sql
{
    public class SqlEventStore : IEventStore
    {
        #region Private Variables

        private string _connectionString;
        private IEventTableNameProvider _eventTableProvider;
        private IJsonSerializer _jsonSerializer;
        private IDbConnectionFactory _connectionFactory;
        private IAggregateRootTypeProvider _aggregateRootTypeProvider;
        private ILogger _logger;

        #endregion

        #region Constructors

        public SqlEventStore(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }
            _connectionString = connectionString;
            _eventTableProvider = ObjectContainer.Resolve<IEventTableNameProvider>();
            _jsonSerializer = ObjectContainer.Resolve<IJsonSerializer>();
            _connectionFactory = ObjectContainer.Resolve<IDbConnectionFactory>();
            _aggregateRootTypeProvider = ObjectContainer.Resolve<IAggregateRootTypeProvider>();
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().Name);
        }

        #endregion

        public void Append(EventStream stream)
        {
            if (stream == null)
            {
                return;
            }

            var aggregateRootType = _aggregateRootTypeProvider.GetAggregateRootType(stream.AggregateRootName);
            var connection = _connectionFactory.CreateConnection(_connectionString);
            var eventTable = _eventTableProvider.GetTable(stream.AggregateRootId, aggregateRootType);

            try
            {
                connection.Open();
                connection.Insert(BuildSqlEventStreamFrom(stream), eventTable);
            }
            catch (SqlException)
            {
                if (connection.State == ConnectionState.Open)
                {
                    var count = connection.GetCount(
                        new
                        {
                            AggregateRootId = stream.AggregateRootId,
                            Version = stream.Version
                        }, eventTable);
                    if (count > 0)
                    {
                        throw new ConcurrentException();
                    }
                    else
                    {
                        throw;
                    }
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                connection.Close();
            }
        }
        public IEnumerable<EventStream> Query(string aggregateRootId, Type aggregateRootType, long minStreamVersion, long maxStreamVersion)
        {
            return _connectionFactory.CreateConnection(_connectionString).TryExecute<IEnumerable<EventStream>>((connection) =>
            {
                var eventTable = _eventTableProvider.GetTable(aggregateRootId, aggregateRootType);
                var sql = string.Format("select * from [{0}] where AggregateRootId = @AggregateRootId and Version >= @MinStreamVersion and Version <= @MaxStreamVersion order by Version asc", eventTable);

                var sqlEventStreams = connection.Query<SqlEventStream>(sql,
                new
                {
                    AggregateRootId = aggregateRootId,
                    MinStreamVersion = minStreamVersion,
                    MaxStreamVersion = maxStreamVersion
                });

                var streams = new List<EventStream>();

                foreach (var sqlEventStream in sqlEventStreams)
                {
                    streams.Add(BuildEventStreamFrom(sqlEventStream));
                }

                return streams;
            });
        }
        public bool IsEventStreamExist(string aggregateRootId, Type aggregateRootType, Guid id)
        {
            return _connectionFactory.CreateConnection(_connectionString).TryExecute<bool>((connection) =>
            {
                var eventTable = _eventTableProvider.GetTable(aggregateRootId, aggregateRootType);
                var count = connection.GetCount(new { Id = id }, eventTable);
                return count > 0;
            });
        }
        public IEnumerable<EventStream> QueryAll()
        {
            return _connectionFactory.CreateConnection(_connectionString).TryExecute<IEnumerable<EventStream>>((connection) =>
            {
                var eventTables = _eventTableProvider.GetAllTables();
                var streams = new List<EventStream>();

                foreach (var eventTable in eventTables)
                {
                    var sql = string.Format("select * from [{0}] order by AggregateRootId, Version asc", eventTable);
                    var sqlEventStreams = connection.Query<SqlEventStream>(sql);

                    foreach (var sqlEventStream in sqlEventStreams)
                    {
                        streams.Add(BuildEventStreamFrom(sqlEventStream));
                    }
                }

                return streams;
            });
        }

        private EventStream BuildEventStreamFrom(SqlEventStream sqlEventStream)
        {
            return new EventStream(
                    sqlEventStream.Id,
                    sqlEventStream.AggregateRootId,
                    sqlEventStream.AggregateRootName,
                    sqlEventStream.Version,
                    sqlEventStream.CommandId,
                    sqlEventStream.Timestamp,
                    _jsonSerializer.Deserialize<IEnumerable<IEvent>>(sqlEventStream.Events));
        }
        private SqlEventStream BuildSqlEventStreamFrom(EventStream eventStream)
        {
            return new SqlEventStream
            {
                Id = eventStream.Id,
                AggregateRootId = eventStream.AggregateRootId,
                AggregateRootName = eventStream.AggregateRootName,
                CommandId = eventStream.CommandId,
                Version = eventStream.Version,
                Timestamp = eventStream.Timestamp,
                Events = _jsonSerializer.Serialize(eventStream.Events)
            };
        }

        class SqlEventStream
        {
            public Guid Id { get; set; }
            public string AggregateRootId { get; set; }
            public string AggregateRootName { get; set; }
            public Guid CommandId { get; set; }
            public long Version { get; set; }
            public DateTime Timestamp { get; set; }
            public string Events { get; set; }
        }
    }
}
