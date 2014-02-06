using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using ECommon.IoC;
using ECommon.Serializing;
using ENode.Domain;
using ENode.Infrastructure;
using ENode.Infrastructure.Dapper;
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

        /// <summary>Commit the event stream to the event store.
        /// </summary>
        /// <param name="stream"></param>
        public EventCommitStatus Commit(EventStream stream)
        {
            return EventCommitStatus.Success;
            //var aggregateRootType = _aggregateRootTypeProvider.GetAggregateRootType(stream.AggregateRootName);
            //var connection = _connectionFactory.CreateConnection(_connectionString);
            //var eventTable = _eventTableProvider.GetTable(stream.AggregateRootId, aggregateRootType);

            //try
            //{
            //    connection.Open();
            //    connection.Insert(BuildSqlEventStreamFrom(stream), eventTable);
            //}
            //catch (SqlException)
            //{
            //    if (connection.State == ConnectionState.Open)
            //    {
            //        var count = connection.GetCount(
            //            new
            //            {
            //                AggregateRootId = stream.AggregateRootId,
            //                Version = stream.Version
            //            }, eventTable);
            //        if (count > 0)
            //        {
            //            throw new ConcurrentException();
            //        }
            //        else
            //        {
            //            throw;
            //        }
            //    }
            //    else
            //    {
            //        throw;
            //    }
            //}
            //finally
            //{
            //    connection.Close();
            //}
        }
        /// <summary>Query event streams from event store.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        /// <param name="aggregateRootType"></param>
        /// <param name="minStreamVersion"></param>
        /// <param name="maxStreamVersion"></param>
        /// <returns></returns>
        public IEnumerable<EventStream> Query(object aggregateRootId, string aggregateRootName, long minStreamVersion, long maxStreamVersion)
        {
            return _connectionFactory.CreateConnection(_connectionString).TryExecute<IEnumerable<EventStream>>((connection) =>
            {
                var eventTable = _eventTableProvider.GetTable(aggregateRootId, aggregateRootName);
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
                    sqlEventStream.CommandId,
                    sqlEventStream.AggregateRootId,
                    sqlEventStream.AggregateRootName,
                    sqlEventStream.Version,
                    sqlEventStream.Timestamp,
                    _jsonSerializer.Deserialize<IEnumerable<IDomainEvent>>(sqlEventStream.Events));
        }
        private SqlEventStream BuildSqlEventStreamFrom(EventStream eventStream)
        {
            return new SqlEventStream
            {
                CommandId = eventStream.CommandId,
                AggregateRootId = eventStream.AggregateRootId,
                AggregateRootName = eventStream.AggregateRootName,
                Version = eventStream.Version,
                Timestamp = eventStream.Timestamp,
                Events = _jsonSerializer.Serialize(eventStream.Events)
            };
        }

        class SqlEventStream
        {
            public Guid CommandId { get; set; }
            public object AggregateRootId { get; set; }
            public string AggregateRootName { get; set; }
            public long Version { get; set; }
            public DateTime Timestamp { get; set; }
            public string Events { get; set; }
        }

        #endregion
    }
}
