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

        public long Append(EventByteStream stream)
        {
            var sqlEventStream = BuildSqlEventStreamFrom(stream);
            return _connectionFactory.CreateConnection(_connectionString).TryExecute<long>(connection =>
            {
                return connection.Insert(sqlEventStream, _commitLogTable);
            });
        }
        public EventByteStream Get(long sequence)
        {
            return _connectionFactory.CreateConnection(_connectionString).TryExecute<EventByteStream>(connection =>
            {
                var sqlEventStream = connection.QuerySingleOrDefault<SqlEventStream>(new { CommitSequence = sequence }, _commitLogTable);
                if (sqlEventStream != null)
                {
                    return BuildEventStreamFrom(sqlEventStream);
                }
                return null;
            });
        }
        public IEnumerable<CommitRecord> Query(long startSequence, int count)
        {
            return _connectionFactory.CreateConnection(_connectionString).TryExecute(connection =>
            {
                var sqlEventStreams = connection.QueryByStartAndSize<SqlCommitRecord>(null, _commitLogTable, "*", "Sequence", startSequence, count);
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
                    sqlCommitRecord.CommitId,
                    sqlCommitRecord.AggregateRootId,
                    sqlCommitRecord.Version);
        }
        private EventByteStream BuildEventStreamFrom(SqlEventStream sqlEventStream)
        {
            return new EventByteStream(
                    sqlEventStream.CommitId,
                    sqlEventStream.AggregateRootId,
                    sqlEventStream.AggregateRootName,
                    sqlEventStream.Version,
                    sqlEventStream.Timestamp,
                    _binarySerializer.Deserialize<IEnumerable<EventEntry>>(sqlEventStream.Events));
        }
        private SqlEventStream BuildSqlEventStreamFrom(EventByteStream eventStream)
        {
            return new SqlEventStream
            {
                CommitId = eventStream.CommitId,
                AggregateRootId = eventStream.AggregateRootId.ToString(),
                AggregateRootName = eventStream.AggregateRootName,
                Version = eventStream.Version,
                Timestamp = eventStream.Timestamp,
                Events = _binarySerializer.Serialize(eventStream.Events)
            };
        }

        class SqlEventStream
        {
            public string CommitId { get; set; }
            public string AggregateRootId { get; set; }
            public string AggregateRootName { get; set; }
            public int Version { get; set; }
            public DateTime Timestamp { get; set; }
            public byte[] Events { get; set; }
        }
        class SqlCommitRecord
        {
            public long Sequence { get; set; }
            public string CommitId { get; set; }
            public string AggregateRootId { get; set; }
            public int Version { get; set; }
        }

        #endregion
    }
}
