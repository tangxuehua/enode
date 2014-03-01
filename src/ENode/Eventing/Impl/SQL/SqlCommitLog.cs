using System;
using System.Collections.Generic;
using ECommon.IoC;
using ECommon.Serializing;
using ENode.Infrastructure.Dapper;
using ENode.Infrastructure.Sql;

namespace ENode.Eventing.Impl.SQL
{
    /// <summary>The SQL implementation of ICommitLog.
    /// </summary>
    public class SqlCommitLog : ICommitLog
    {
        #region Private Variables

        private readonly string _connectionString;
        private readonly string _commitLogTable;
        private readonly IBinarySerializer _binarySerializer;
        private readonly IDbConnectionFactory _connectionFactory;

        #endregion

        #region Constructors

        /// <summary>Parameterized constructor.
        /// </summary>
        public SqlCommitLog(string connectionString, string commitLogTable)
        {
            _connectionString = connectionString;
            _commitLogTable = commitLogTable;
            _binarySerializer = ObjectContainer.Resolve<IBinarySerializer>();
            _connectionFactory = ObjectContainer.Resolve<IDbConnectionFactory>();
        }

        #endregion

        #region Public Methods

        public long Append(EventStream stream)
        {
            var sqlEventStream = BuildSqlEventStreamFrom(stream);
            return _connectionFactory.CreateConnection(_connectionString).TryExecute<long>(connection =>
            {
                return connection.Insert(sqlEventStream, _commitLogTable);
            });
        }
        public EventStream Get(long commitSequence)
        {
            return _connectionFactory.CreateConnection(_connectionString).TryExecute<EventStream>(connection =>
            {
                var sqlEventStream = connection.QuerySingleOrDefault<SqlEventStream>(new { CommitSequence = commitSequence }, _commitLogTable);
                if (sqlEventStream != null)
                {
                    return BuildEventStreamFrom(sqlEventStream);
                }
                return null;
            });
        }
        public IEnumerable<CommitRecord> Query(long startSequence, int size)
        {
            return _connectionFactory.CreateConnection(_connectionString).TryExecute(connection =>
            {
                var sqlEventStreams = connection.QueryByStartAndSize<SqlCommitRecord>(null, _commitLogTable, "*", "CommitSequence", startSequence, size);
                var commitRecords = new List<CommitRecord>();
                foreach (var sqlEventStream in sqlEventStreams)
                {
                    commitRecords.Add(BuildCommitRecordFrom(sqlEventStream));
                }
                return commitRecords;
            });
        }

        #endregion

        #region Private Methods

        private CommitRecord BuildCommitRecordFrom(SqlCommitRecord sqlCommitRecord)
        {
            return new CommitRecord(
                    sqlCommitRecord.Version,
                    sqlCommitRecord.CommandId,
                    sqlCommitRecord.AggregateRootId,
                    sqlCommitRecord.Version);
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
                AggregateRootId = eventStream.AggregateRootId.ToString(),
                AggregateRootName = eventStream.AggregateRootName,
                Version = eventStream.Version,
                Timestamp = eventStream.Timestamp,
                Events = _binarySerializer.Serialize(eventStream.Events)
            };
        }

        class SqlEventStream
        {
            public Guid CommandId { get; set; }
            public string AggregateRootId { get; set; }
            public string AggregateRootName { get; set; }
            public long Version { get; set; }
            public DateTime Timestamp { get; set; }
            public byte[] Events { get; set; }
        }
        class SqlCommitRecord
        {
            public long CommitSequence { get; set; }
            public Guid CommandId { get; set; }
            public string AggregateRootId { get; set; }
            public long Version { get; set; }
        }

        #endregion
    }
}
