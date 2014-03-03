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
        private readonly ConcurrentDictionary<long, EventByteStream> _commitLogDict = new ConcurrentDictionary<long, EventByteStream>();

        public long Append(EventByteStream stream)
        {
            var nextSequence = Interlocked.Increment(ref _sequence);
            if (!_commitLogDict.TryAdd(nextSequence, stream))
            {
                throw new ENodeException("Append event commit log failed.");
            }
            return nextSequence;
        }
        public EventByteStream Get(long sequence)
        {
            EventByteStream eventStream;
            if (_commitLogDict.TryGetValue(sequence, out eventStream))
            {
                return eventStream;
            }
            return null;
        }
        public IEnumerable<CommitRecord> Query(long startSequence, int count)
        {
            var commitRecords = new List<CommitRecord>();
            var currentSequnece = startSequence;
            var eventStream = Get(currentSequnece);

            while (eventStream != null)
            {
                commitRecords.Add(new CommitRecord(currentSequnece, eventStream.CommitId, eventStream.AggregateRootId, eventStream.Version));
                if (commitRecords.Count == count) break;
                eventStream = Get(currentSequnece++);
            }

            return commitRecords;
        }
    }
}
