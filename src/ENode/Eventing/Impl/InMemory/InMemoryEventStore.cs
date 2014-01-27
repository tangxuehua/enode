using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ENode.Infrastructure;

namespace ENode.Eventing.Impl.InMemory
{
    /// <summary>In-memory concurrent dictionary based event store implementation.
    /// </summary>
    public class InMemoryEventStore : IEventStore
    {
        private readonly ConcurrentDictionary<AggregateKey, AggregateVersionInfo> _aggregateVersionDict = new ConcurrentDictionary<AggregateKey, AggregateVersionInfo>();
        private readonly ConcurrentDictionary<AggregateKey, IList<EventStream>> _aggregateEventsDict = new ConcurrentDictionary<AggregateKey, IList<EventStream>>();

        /// <summary>Append the event stream to the event store.
        /// </summary>
        /// <param name="stream"></param>
        public void Append(EventStream stream)
        {
            if (stream == null) return;

            var aggregateKey = new AggregateKey(stream.AggregateRootId);

            if (stream.Version == 1)
            {
                _aggregateVersionDict.TryAdd(aggregateKey, new AggregateVersionInfo { CurrentVersion = 1 });
                _aggregateEventsDict.TryAdd(aggregateKey, new List<EventStream>());
                return;
            }

            var aggregateVersionInfo = _aggregateVersionDict[aggregateKey];

            if (aggregateVersionInfo.CurrentVersion >= stream.Version)
            {
                if (!_aggregateEventsDict[aggregateKey].Any(x => x.Id == stream.Id))
                {
                    throw new ConcurrentException("");
                }
            }

            var originalStatus = Interlocked.CompareExchange(ref aggregateVersionInfo.Status, AggregateVersionInfo.Editing, AggregateVersionInfo.UnEditing);
            var hasConcurrentException = false;
            if (originalStatus != aggregateVersionInfo.Status)
            {
                if (stream.Version == aggregateVersionInfo.CurrentVersion + 1)
                {
                    _aggregateEventsDict[aggregateKey].Add(stream);
                    aggregateVersionInfo.CurrentVersion++;
                }
                else
                {
                    hasConcurrentException = true;
                }

                Interlocked.Exchange(ref aggregateVersionInfo.Status, AggregateVersionInfo.UnEditing);
            }
            else
            {
                hasConcurrentException = true;
            }

            if (hasConcurrentException)
            {
                throw new ConcurrentException("");
            }
        }
        /// <summary>Check whether an event stream is exist in the event store.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        /// <param name="aggregateRootType"></param>
        /// <param name="minStreamVersion"></param>
        /// <param name="maxStreamVersion"></param>
        /// <returns></returns>
        public IEnumerable<EventStream> Query(object aggregateRootId, Type aggregateRootType, long minStreamVersion, long maxStreamVersion)
        {
            return _aggregateEventsDict[new AggregateKey(aggregateRootId)].Where(x => x.Version >= minStreamVersion && x.Version <= maxStreamVersion).OrderBy(x => x.Version);
        }
        /// <summary>Query all the event streams from the event store.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<EventStream> QueryAll()
        {
            var totalStreams = new List<EventStream>();
            foreach (var streams in _aggregateEventsDict.Values)
            {
                totalStreams.AddRange(streams);
            }
            return totalStreams;
        }

        class AggregateKey
        {
            private object AggregateRootId { get; set; }

            public AggregateKey(object aggregateRootId)
            {
                AggregateRootId = aggregateRootId;
            }

            public override bool Equals(object obj)
            {
                var aggregateKey = obj as AggregateKey;
                if (aggregateKey == null)
                {
                    return false;
                }
                if (aggregateKey == this)
                {
                    return true;
                }
                return object.Equals(AggregateRootId, aggregateKey.AggregateRootId);
            }
            public override int GetHashCode()
            {
                return AggregateRootId.GetHashCode();
            }
        }
    }
}
