using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using ECommon.Components;
using ECommon.Dapper;
using ECommon.Serializing;
using ENode.Infrastructure;

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
        }

        #endregion

        #region Public Methods

        public EventAppendResult Append(EventCommitRecord record)
        {
            var commitRecord = ConvertTo(record);

            using (var connection = GetConnection())
            {
                connection.Open();
                try
                {
                    connection.Insert(commitRecord, _eventTable);
                    return EventAppendResult.Success;
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2601)
                    {
                        if (ex.Message.Contains(_commitIndexName))
                        {
                            return EventAppendResult.DuplicateCommit;
                        }
                        if (ex.Message.Contains(_versionIndexName))
                        {
                            if (commitRecord.Version == 1)
                            {
                                throw new DuplicateAggregateException(commitRecord.AggregateRootTypeCode, commitRecord.AggregateRootId);
                            }
                            throw new ConcurrentException();
                        }
                    }
                    throw;
                }
            }
        }
        public EventCommitRecord Find(string aggregateRootId, string commitId)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                var commitRecord = connection.QueryList<SqlEventCommitRecord>(new { AggregateRootId = aggregateRootId, CommitId = commitId }, _eventTable).SingleOrDefault();
                if (commitRecord != null)
                {
                    return ConvertFrom(commitRecord);
                }
                return null;
            }
        }
        public IEnumerable<EventCommitRecord> QueryAggregateEvents(string aggregateRootId, int aggregateRootTypeCode, int minVersion, int maxVersion)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
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
            }
        }
        public IEnumerable<EventCommitRecord> QueryByPage(int pageIndex, int pageSize)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                var commitRecords = connection.QueryPaged<SqlEventCommitRecord>(null, _eventTable, "Sequence", pageIndex, pageSize);
                var records = new List<EventCommitRecord>();
                foreach (var commitRecord in commitRecords)
                {
                    records.Add(ConvertFrom(commitRecord));
                }
                return records;
            }
        }

        #endregion

        #region Private Methods

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
        private EventCommitRecord ConvertFrom(SqlEventCommitRecord commitRecord)
        {
            return new EventCommitRecord(
                    commitRecord.CommitId,
                    commitRecord.AggregateRootId,
                    commitRecord.AggregateRootTypeCode,
                    commitRecord.ProcessId,
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
                AggregateRootTypeCode = eventStream.AggregateRootTypeCode,
                ProcessId = eventStream.ProcessId,
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
            public int AggregateRootTypeCode { get; set; }
            public string ProcessId { get; set; }
            public int Version { get; set; }
            public DateTime Timestamp { get; set; }
            public byte[] Events { get; set; }
        }
    }
}
