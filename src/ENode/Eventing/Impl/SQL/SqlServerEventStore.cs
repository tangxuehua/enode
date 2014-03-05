using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using ECommon.IoC;
using ECommon.Serializing;
using ENode.Infrastructure;
using ENode.Infrastructure.Dapper;
using ENode.Infrastructure.Sql;

namespace ENode.Eventing.Impl.SQL
{
    /// <summary>The Microsoft SqlServer based implementation of IEventStore.
    /// </summary>
    public class SqlServerEventStore : IEventStore
    {
        #region Private Variables

        private readonly string _connectionString;
        private readonly string _eventTable;
        private readonly string _commitIndexName;
        private readonly string _versionIndexName;
        private readonly IBinarySerializer _binarySerializer;
        private readonly IDbConnectionFactory _connectionFactory;

        #endregion

        #region Constructors

        /// <summary>Parameterized constructor.
        /// </summary>
        public SqlServerEventStore(string connectionString, string eventTable, string commitIndexName, string versionIndexName)
        {
            _connectionString = connectionString;
            _eventTable = eventTable;
            _commitIndexName = commitIndexName;
            _versionIndexName = versionIndexName;
            _binarySerializer = ObjectContainer.Resolve<IBinarySerializer>();
            _connectionFactory = ObjectContainer.Resolve<IDbConnectionFactory>();
        }

        #endregion

        #region Public Methods

        public EventAppendResult Append(EventCommitRecord record)
        {
            var commitRecord = ConvertTo(record);
            return _connectionFactory.CreateConnection(_connectionString).TryExecute<EventAppendResult>(connection =>
            {
                try
                {
                    connection.Insert(commitRecord, _eventTable);
                    return EventAppendResult.Success;
                }
                catch (SqlException sqlException)
                {
                    if (sqlException.Number == 2601)
                    {
                        if (sqlException.Message.Contains(_commitIndexName))
                        {
                            return EventAppendResult.DuplicateCommit;
                        }
                        if (sqlException.Message.Contains(_versionIndexName))
                        {
                            if (commitRecord.Version == 1)
                            {
                                throw new DuplicateAggregateException(commitRecord.AggregateRootName, commitRecord.AggregateRootId);
                            }
                            throw new ConcurrentException();
                        }
                    }
                    throw;
                }
            });
        }
        public EventCommitRecord Find(string aggregateRootId, string commitId)
        {
            return _connectionFactory.CreateConnection(_connectionString).TryExecute<EventCommitRecord>(connection =>
            {
                var commitRecord = connection.QuerySingleOrDefault<SqlEventCommitRecord>(new { AggregateRootId = aggregateRootId, CommitId = commitId }, _eventTable);
                if (commitRecord != null)
                {
                    return ConvertFrom(commitRecord);
                }
                return null;
            });
        }
        public IEnumerable<EventCommitRecord> QueryAggregateEvents(string aggregateRootId, string aggregateRootName, int minVersion, int maxVersion)
        {
            return _connectionFactory.CreateConnection(_connectionString).TryExecute(connection =>
            {
                var sql = string.Format("SELECT * FROM [{0}] WHERE AggregateRootId = @AggregateRootId AND Version >= @MinVersion AND Version <= @MaxVersion", _eventTable);
                var commitRecords = connection.Query<SqlEventCommitRecord>(sql,
                new
                {
                    AggregateRootId = aggregateRootId,
                    MinVersion = minVersion,
                    MaxVersion = maxVersion
                });
                var records = new List<EventCommitRecord>();
                foreach (var commitRecord in commitRecords)
                {
                    records.Add(ConvertFrom(commitRecord));
                }
                return records;
            });
        }
        public IEnumerable<EventCommitRecord> QueryByPage(int pageIndex, int pageSize)
        {
            return _connectionFactory.CreateConnection(_connectionString).TryExecute(connection =>
            {
                var commitRecords = connection.QueryPaged<SqlEventCommitRecord>(null, _eventTable, "*", "Sequence", pageIndex, pageSize);
                var records = new List<EventCommitRecord>();
                foreach (var commitRecord in commitRecords)
                {
                    records.Add(ConvertFrom(commitRecord));
                }
                return records;
            });
        }

        #endregion

        #region Private Methods

        private EventCommitRecord ConvertFrom(SqlEventCommitRecord commitRecord)
        {
            return new EventCommitRecord(
                    commitRecord.CommitId,
                    commitRecord.AggregateRootId,
                    commitRecord.AggregateRootName,
                    commitRecord.Version,
                    commitRecord.Timestamp,
                    _binarySerializer.Deserialize<IEnumerable<EventEntry>>(commitRecord.Events));
        }
        private SqlEventCommitRecord ConvertTo(EventCommitRecord eventStream)
        {
            return new SqlEventCommitRecord
            {
                CommitId = eventStream.CommitId,
                AggregateRootId = eventStream.AggregateRootId,
                AggregateRootName = eventStream.AggregateRootName,
                Version = eventStream.Version,
                Timestamp = eventStream.Timestamp,
                Events = _binarySerializer.Serialize(eventStream.Events)
            };
        }

        #endregion

        class SqlEventCommitRecord
        {
            public string CommitId { get; set; }
            public string AggregateRootId { get; set; }
            public string AggregateRootName { get; set; }
            public int Version { get; set; }
            public DateTime Timestamp { get; set; }
            public byte[] Events { get; set; }
        }
    }
}
