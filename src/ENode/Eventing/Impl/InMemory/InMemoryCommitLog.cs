using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using ENode.Infrastructure;

namespace ENode.Eventing.Impl.InMemory
{
    /// <summary>In-memory based commit log implementation. It is only used for unit test.
    /// </summary>
    public class InMemoryCommitLog : ICommitLog
    {
        private long _sequence;
        private readonly ConcurrentDictionary<long, EventStream> _commitLogDict = new ConcurrentDictionary<long, EventStream>();

        public long Append(EventStream stream)
        {
            var nextSequence = Interlocked.Increment(ref _sequence);
            if (!_commitLogDict.TryAdd(nextSequence, stream))
            {
                throw new ENodeException("Append event commit log failed.");
            }
            return nextSequence;
        }
        public EventStream Get(long commitSequence)
        {
            EventStream eventStream;
            if (_commitLogDict.TryGetValue(commitSequence, out eventStream))
            {
                return eventStream;
            }
            return null;
        }
        public IEnumerable<CommitRecord> Query(long startSequence, int size)
        {
            var commitRecords = new List<CommitRecord>();
            var currentSequnece = startSequence;
            var eventStream = Get(currentSequnece);

            while (eventStream != null)
            {
                commitRecords.Add(new CommitRecord(currentSequnece, eventStream.CommandId, eventStream.AggregateRootId, eventStream.Version));
                if (commitRecords.Count == size) break;
                eventStream = Get(currentSequnece++);
            }

            return commitRecords;
        }
    }
}
