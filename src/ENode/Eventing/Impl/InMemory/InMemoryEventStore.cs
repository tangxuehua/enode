using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ECommon.Logging;
using ENode.Infrastructure;

namespace ENode.Eventing.Impl.InMemory
{
    /// <summary>In-memory based event store implementation. It is only used for unit test.
    /// </summary>
    public class InMemoryEventStore : IEventStore
    {
        private const int Editing = 1;
        private const int UnEditing = 0;
        private readonly ConcurrentDictionary<string, AggregateInfo> _aggregateInfoDict;
        private readonly ConcurrentDictionary<long, EventCommitRecord> _eventDict;
        private readonly ILogger _logger;
        private long _sequence;

        public InMemoryEventStore(ILoggerFactory loggerFactory)
        {
            _aggregateInfoDict = new ConcurrentDictionary<string, AggregateInfo>();
            _eventDict = new ConcurrentDictionary<long, EventCommitRecord>();
            _logger = loggerFactory.Create(GetType().FullName);
        }

        public EventAppendResult Append(EventCommitRecord commitRecord)
        {
            var aggregateInfo = _aggregateInfoDict.GetOrAdd(commitRecord.AggregateRootId, new AggregateInfo());
            var originalStatus = Interlocked.CompareExchange(ref aggregateInfo.Status, Editing, UnEditing);

            if (originalStatus == aggregateInfo.Status)
            {
                throw new ConcurrentException();
            }

            try
            {
                if (aggregateInfo.CommitDict.ContainsKey(commitRecord.CommitId))
                {
                    return EventAppendResult.DuplicateCommit;
                }
                if (commitRecord.Version == aggregateInfo.CurrentVersion + 1)
                {
                    aggregateInfo.CommitDict[commitRecord.CommitId] = commitRecord;
                    aggregateInfo.EventDict[commitRecord.Version] = commitRecord;
                    aggregateInfo.CurrentVersion = commitRecord.Version;
                    _eventDict.TryAdd(Interlocked.Increment(ref _sequence), commitRecord);
                    return EventAppendResult.Success;
                }
                else if (commitRecord.Version == 1)
                {
                    throw new DuplicateAggregateException(commitRecord.AggregateRootTypeCode, commitRecord.AggregateRootId);
                }
                else
                {
                    throw new ConcurrentException();
                }
            }
            finally
            {
                Interlocked.Exchange(ref aggregateInfo.Status, UnEditing);
            }
        }
        public EventCommitRecord Find(string aggregateRootId, string commitId)
        {
            AggregateInfo aggregateInfo;
            if (!_aggregateInfoDict.TryGetValue(aggregateRootId, out aggregateInfo))
            {
                return null;
            }

            EventCommitRecord commitRecord;
            return aggregateInfo.CommitDict.TryGetValue(commitId, out commitRecord) ? commitRecord : null;
        }
        public IEnumerable<EventCommitRecord> QueryAggregateEvents(string aggregateRootId, int aggregateRootTypeCode, int minVersion, int maxVersion)
        {
            var commitRecords = new List<EventCommitRecord>();

            AggregateInfo aggregateInfo;
            if (!_aggregateInfoDict.TryGetValue(aggregateRootId, out aggregateInfo))
            {
                return commitRecords;
            }

            var min = minVersion > 1 ? minVersion : 1;
            var max = maxVersion < aggregateInfo.CurrentVersion ? maxVersion : aggregateInfo.CurrentVersion;

            return aggregateInfo.EventDict.Where(x => x.Key >= min && x.Key <= max).Select(x => x.Value).ToList();
        }
        public IEnumerable<EventCommitRecord> QueryByPage(int pageIndex, int pageSize)
        {
            var start = pageIndex * pageSize;
            return _eventDict.Skip(start).Take(pageSize).Select(x => x.Value).ToList();
        }

        class AggregateInfo
        {
            public int Status;
            public long CurrentVersion;
            public ConcurrentDictionary<int, EventCommitRecord> EventDict = new ConcurrentDictionary<int, EventCommitRecord>();
            public ConcurrentDictionary<string, EventCommitRecord> CommitDict = new ConcurrentDictionary<string, EventCommitRecord>();
        }
    }
}
