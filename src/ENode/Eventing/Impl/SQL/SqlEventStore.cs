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

        private readonly SqlEventStoreSetting _setting;
        private readonly IBinarySerializer _binarySerializer;
        private readonly IDbConnectionFactory _connectionFactory;

        #endregion

        #region Constructors

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="setting"></param>
        public SqlEventStore(SqlEventStoreSetting setting)
        {
            _setting = setting;
            _binarySerializer = ObjectContainer.Resolve<IBinarySerializer>();
            _connectionFactory = ObjectContainer.Resolve<IDbConnectionFactory>();
        }

        #endregion

        #region Public Methods

        /// <summary>Commit the event stream to the event store.
        /// </summary>
        /// <param name="stream"></param>
        public EventCommitStatus Commit(EventStream stream)
        {
            return EventCommitStatus.Success;
            //var connection = _connectionFactory.CreateConnection(_setting.ConnectionString);
            //var sqlEventStream = BuildSqlEventStreamFrom(stream);

            //try
            //{
            //    //Open the connection.
            //    connection.Open();

            //    //Append the commit log, and returns the commit sequence.
            //    var commitSequence = connection.Insert(sqlEventStream, _setting.CommitLogTable);


                
            //    return EventCommitStatus.Success;
            //}
            //catch
            //{
            //    if (connection.State == ConnectionState.Open)
            //    {
            //        var isDuplicateCommit = connection.GetCount(new { CommandId = stream.CommandId }, eventTable) > 0;
            //        if (isDuplicateCommit)
            //        {
            //            return EventCommitStatus.DuplicateCommit;
            //        }

            //        var isDuplicateVersion = connection.GetCount(new { AggregateRootId = stream.AggregateRootId, Version = stream.Version }, eventTable) > 0;
            //        if (isDuplicateVersion)
            //        {
            //            if (stream.Version == 1)
            //            {
            //                throw new DuplicateAggregateException("Aggregate [name={0},id={1}] has already been created.", stream.AggregateRootName, stream.AggregateRootId);
            //            }
            //            else
            //            {
            //                throw new ConcurrentException();
            //            }
            //        }

            //        throw;
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
            return null;
            //return _connectionFactory.CreateConnection(_connectionString).TryExecute<IEnumerable<EventStream>>((connection) =>
            //{
            //    var eventTable = _eventTableProvider.GetTable(aggregateRootId, aggregateRootName);
            //    var sql = string.Format("select * from [{0}] where AggregateRootId = @AggregateRootId and Version >= @MinStreamVersion and Version <= @MaxStreamVersion order by Version asc", eventTable);

            //    var sqlEventStreams = connection.Query<SqlEventStream>(sql,
            //    new
            //    {
            //        AggregateRootId = aggregateRootId,
            //        MinStreamVersion = minStreamVersion,
            //        MaxStreamVersion = maxStreamVersion
            //    });

            //    return sqlEventStreams.Select(BuildEventStreamFrom).ToList();
            //});
        }
        /// <summary>Query all the event streams from the event store.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<EventStream> QueryAll()
        {
            return null;
            //return _connectionFactory.CreateConnection(_connectionString).TryExecute<IEnumerable<EventStream>>(connection =>
            //{
            //    var eventTables = _eventTableProvider.GetAllTables();
            //    var streams = new List<EventStream>();

            //    foreach (var sqlEventStreams in eventTables.Select(eventTable => string.Format("select * from [{0}] order by AggregateRootId, Version asc", eventTable)).Select(sql => connection.Query<SqlEventStream>(sql)))
            //    {
            //        streams.AddRange(sqlEventStreams.Select(BuildEventStreamFrom));
            //    }

            //    return streams;
            //});
        }

        #endregion

        #region Private Methods

        private void AppendCommitLog(SqlEventStream sqlEventStream)
        {

        }
        private EventStream BuildEventStreamFrom(SqlEventStream sqlEventStream)
        {
            return new EventStream(
                    sqlEventStream.CommandId,
                    sqlEventStream.AggregateRootId,
                    sqlEventStream.AggregateRootName,
                    sqlEventStream.Version,
                    sqlEventStream.Timestamp,
                    _binarySerializer.Deserialize<IEnumerable<IDomainEvent>>(sqlEventStream.Events));
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
                Events = _binarySerializer.Serialize(eventStream.Events)
            };
        }

        class SqlEventStream
        {
            public Guid CommandId { get; set; }
            public object AggregateRootId { get; set; }
            public string AggregateRootName { get; set; }
            public long Version { get; set; }
            public DateTime Timestamp { get; set; }
            public byte[] Events { get; set; }
        }

        #endregion

        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Shutdown()
        {
            throw new NotImplementedException();
        }
    }
}
