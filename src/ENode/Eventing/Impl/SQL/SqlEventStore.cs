using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using ENode.Domain;
using ENode.Infrastructure;
using ENode.Infrastructure.Concurrent;
using ENode.Infrastructure.Dapper;
using ENode.Infrastructure.Serializing;
using ENode.Infrastructure.Sql;

namespace ENode.Eventing.Impl.SQL
{
    /// <summary>The SQL implementation of IEventStore.
    /// </summary>
    public class SqlEventStore : IEventStore
    {
        #region Private Variables

        private readonly string _connectionString;
        private readonly IEventTableNameProvider _eventTableProvider;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly IAggregateRootTypeProvider _aggregateRootTypeProvider;

        #endregion

        #region Constructors

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <exception cref="ArgumentNullException"></exception>
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
        }

        #endregion

        #region Public Methods

        /// <summary>Append the event stream to the event store.
        /// </summary>
        /// <param name="stream"></param>
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
        /// <summary>Query event streams from event store.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        /// <param name="aggregateRootType"></param>
        /// <param name="minStreamVersion"></param>
        /// <param name="maxStreamVersion"></param>
        /// <returns></returns>
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

                return sqlEventStreams.Select(BuildEventStreamFrom).ToList();
            });
        }
        /// <summary>Check whether an event stream is exist in the event store.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        /// <param name="aggregateRootType"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool IsEventStreamExist(string aggregateRootId, Type aggregateRootType, Guid id)
        {
            return _connectionFactory.CreateConnection(_connectionString).TryExecute((connection) =>
            {
                var eventTable = _eventTableProvider.GetTable(aggregateRootId, aggregateRootType);
                var count = connection.GetCount(new { Id = id }, eventTable);
                return count > 0;
            });
        }
        /// <summary>Query all the event streams from the event store.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<EventStream> QueryAll()
        {
            return _connectionFactory.CreateConnection(_connectionString).TryExecute<IEnumerable<EventStream>>(connection =>
            {
                var eventTables = _eventTableProvider.GetAllTables();
                var streams = new List<EventStream>();

                foreach (var sqlEventStreams in eventTables.Select(eventTable => string.Format("select * from [{0}] order by AggregateRootId, Version asc", eventTable)).Select(sql => connection.Query<SqlEventStream>(sql)))
                {
                    streams.AddRange(sqlEventStreams.Select(BuildEventStreamFrom));
                }

                return streams;
            });
        }

        #endregion

        #region Private Methods

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

        #endregion
    }
}
