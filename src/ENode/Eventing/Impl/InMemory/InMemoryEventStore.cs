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
        private readonly ConcurrentDictionary<AggregateKey, ConcurrentDictionary<Guid, EventStream>> _aggregateEventsDict = new ConcurrentDictionary<AggregateKey, ConcurrentDictionary<Guid, EventStream>>();

        /// <summary>Append the event stream to the event store.
        /// </summary>
        /// <param name="stream"></param>
        public void Append(EventStream stream)
        {
            if (stream == null) return;

            var aggregateKey = new AggregateKey(stream.AggregateRootId);

            if (stream.Version == 1)
            {
                if (_aggregateEventsDict.TryAdd(aggregateKey, new ConcurrentDictionary<Guid, EventStream>(new KeyValuePair<Guid, EventStream>[] { new KeyValuePair<Guid, EventStream>(stream.CommandId, stream) })))
                {
                    _aggregateVersionDict.TryAdd(aggregateKey, new AggregateVersionInfo { CurrentVersion = 1 });
                }
                else
                {
                    if (_aggregateEventsDict[aggregateKey].Any(x => x.Key == stream.CommandId))
                    {
                        return;
                    }
                    else
                    {
                        throw new DuplicateAggregateException("Aggregate [key:{0}] already been created.", aggregateKey);
                    }
                }
                return;
            }

            AggregateVersionInfo aggregateVersionInfo;
            if (!_aggregateVersionDict.TryGetValue(aggregateKey, out aggregateVersionInfo))
            {
                throw new KeyNotFoundException(string.Format("Cannot find aggregate version info by key:{0}", aggregateKey));
            }

            if (stream.Version <= aggregateVersionInfo.CurrentVersion)
            {
                if (_aggregateEventsDict[aggregateKey].Any(x => x.Key == stream.CommandId))
                {
                    return;
                }
                else
                {
                    throw new ConcurrentException();
                }
            }

            var originalStatus = Interlocked.CompareExchange(ref aggregateVersionInfo.Status, AggregateVersionInfo.Editing, AggregateVersionInfo.UnEditing);
            var hasConcurrentException = false;
            if (originalStatus != aggregateVersionInfo.Status)
            {
                if (stream.Version == aggregateVersionInfo.CurrentVersion + 1)
                {
                    _aggregateEventsDict[aggregateKey].TryAdd(stream.CommandId, stream);
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
            ConcurrentDictionary<Guid, EventStream> eventStreams;
            if (!_aggregateEventsDict.TryGetValue(new AggregateKey(aggregateRootId), out eventStreams))
            {
                throw new KeyNotFoundException(string.Format("aggregateKey:[{0}]", aggregateRootId));
            }
            return eventStreams.ToArray().Where(x => x.Value.Version >= minStreamVersion && x.Value.Version <= maxStreamVersion).OrderBy(x => x.Value.Version).Select(x => x.Value);
        }
        /// <summary>Query all the event streams from the event store.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<EventStream> QueryAll()
        {
            var totalStreams = new List<EventStream>();
            foreach (var streams in _aggregateEventsDict.Values)
            {
                totalStreams.AddRange(streams.Select(x => x.Value).ToArray());
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
                if (aggregateKey == null || aggregateKey.GetType() != this.GetType())
                {
                    return false;
                }
                if (aggregateKey == this)
                {
                    return true;
                }
                return AggregateRootId.Equals(aggregateKey.AggregateRootId);
            }
            public override int GetHashCode()
            {
                return AggregateRootId.GetHashCode();
            }
            public override string ToString()
            {
                return AggregateRootId.ToString();
            }
        }
        /// <summary>A data structure contains the current version of the aggregate.
        /// </summary>
        class AggregateVersionInfo
        {
            public const int Editing = 1;
            public const int UnEditing = 0;

            public int CurrentVersion = 0;
            public int Status = UnEditing;
        }
    }
}
