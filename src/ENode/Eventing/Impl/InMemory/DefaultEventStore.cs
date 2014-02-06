using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using ENode.Infrastructure;

namespace ENode.Eventing.Impl.InMemory
{
    /// <summary>Default event store implementation.
    /// </summary>
    public class DefaultEventStore : IEventStore
    {
        private long _commitSequence;
        private readonly ConcurrentDictionary<long, EventStream> _commitLogDict = new ConcurrentDictionary<long, EventStream>();
        private readonly ConcurrentDictionary<Guid, long> _commitDict = new ConcurrentDictionary<Guid, long>();
        private readonly ConcurrentDictionary<string, long> _versionIndexDict = new ConcurrentDictionary<string, long>();
        private readonly ConcurrentDictionary<string, long> _aggregateVersionDict = new ConcurrentDictionary<string, long>();

        /// <summary>Commit the given event stream to the event store.
        /// </summary>
        /// <param name="stream"></param>
        public EventCommitStatus Commit(EventStream stream)
        {
            var commitSequence = Interlocked.Increment(ref _commitSequence);
            if (!_commitLogDict.TryAdd(commitSequence, stream))
            {
                throw new ENodeException("Append event commit log failed.");
            }

            if (!_commitDict.TryAdd(stream.CommandId, commitSequence))
            {
                return EventCommitStatus.DuplicateCommit;
            }

            var aggregateRootId = stream.AggregateRootId.ToString();
            if (stream.Version == 1)
            {
                if (!_aggregateVersionDict.TryAdd(aggregateRootId, stream.Version))
                {
                    throw new DuplicateAggregateException("Aggregate [name={0},id={1}] has already been created.", stream.AggregateRootName, stream.AggregateRootId);
                }
            }
            else
            {
                if (!_aggregateVersionDict.TryUpdate(aggregateRootId, stream.Version, stream.Version - 1))
                {
                    throw new ConcurrentException();
                }
            }

            var key = string.Format("{0}-{1}", aggregateRootId, stream.Version);
            _versionIndexDict.TryAdd(key, commitSequence);

            return EventCommitStatus.Success;
        }
        /// <summary>Query event streams from event store.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        /// <param name="aggregateRootName"></param>
        /// <param name="minStreamVersion"></param>
        /// <param name="maxStreamVersion"></param>
        /// <returns></returns>
        public IEnumerable<EventStream> Query(object aggregateRootId, string aggregateRootName, long minStreamVersion, long maxStreamVersion)
        {
            long currentVersion;
            if (_aggregateVersionDict.TryGetValue(aggregateRootId.ToString(), out currentVersion))
            {
                var minVersion = minStreamVersion > 1 ? minStreamVersion : 1;
                var maxVersion = maxStreamVersion < currentVersion ? maxStreamVersion : currentVersion;

                var eventStreamList = new List<EventStream>();
                for (var version = minVersion; version <= maxVersion; version++)
                {
                    var key = string.Format("{0}-{1}", aggregateRootId, version);
                    long commitSequence;
                    if (!_versionIndexDict.TryGetValue(key, out commitSequence))
                    {
                        throw new ENodeException("Event commit sequence cannot be found of aggregate [name={0},id={1},version:{2}].", aggregateRootName, aggregateRootId, version);
                    }
                    EventStream eventStream;
                    if (!_commitLogDict.TryGetValue(commitSequence, out eventStream))
                    {
                        throw new ENodeException("Event stream cannot be found from commit log, commit sequence:{0}, aggregate [name={1},id={2},version:{3}].", commitSequence, aggregateRootName, aggregateRootId, version);
                    }
                    eventStreamList.Add(eventStream);
                }
                return eventStreamList;
            }
            return new EventStream[0];
        }
        /// <summary>Query all the event streams from the event store.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<EventStream> QueryAll()
        {
            var totalStreams = new List<EventStream>();
            //TODO
            //foreach (var streams in _aggregateEventsDict.Values)
            //{
            //    totalStreams.AddRange(streams.Select(x => x.Value).ToArray());
            //}
            return totalStreams;
        }
    }
}
