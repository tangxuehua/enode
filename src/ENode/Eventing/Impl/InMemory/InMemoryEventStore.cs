using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ENode.Infrastructure.Concurrent;

namespace ENode.Eventing.Impl.InMemory
{
    /// <summary>In-memory concurrent dictionary based event store implementation.
    /// </summary>
    public class InMemoryEventStore : IEventStore
    {
        private readonly ConcurrentDictionary<EventKey, EventStream> _eventDict = new ConcurrentDictionary<EventKey, EventStream>();

        /// <summary>Append the event stream to the event store.
        /// </summary>
        /// <param name="stream"></param>
        public void Append(EventStream stream)
        {
            if (stream == null) return;
            if (!_eventDict.TryAdd(new EventKey(stream.AggregateRootId, stream.Version), stream))
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
            return _eventDict
                .Values
                .Where(x => object.Equals(x.AggregateRootId, aggregateRootId) && x.Version >= minStreamVersion && x.Version <= maxStreamVersion)
                .OrderBy(x => x.Version);
        }
        /// <summary>Query event streams from event store.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        /// <param name="aggregateRootType"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool IsEventStreamExist(object aggregateRootId, Type aggregateRootType, Guid id)
        {
            return _eventDict.Values.Any(x => object.Equals(x.AggregateRootId, aggregateRootId) && x.Id == id);
        }
        /// <summary>Query all the event streams from the event store.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<EventStream> QueryAll()
        {
            return _eventDict.Values;
        }

        class EventKey
        {
            private object AggregateRootId { get; set; }
            private long Version { get; set; }

            public EventKey(object aggregateRootId, long version)
            {
                AggregateRootId = aggregateRootId;
                Version = version;
            }

            public override bool Equals(object obj)
            {
                var eventKey = obj as EventKey;

                if (eventKey == null)
                {
                    return false;
                }
                if (eventKey == this)
                {
                    return true;
                }

                return object.Equals(AggregateRootId, eventKey.AggregateRootId) && Version == eventKey.Version;
            }
            public override int GetHashCode()
            {
                return AggregateRootId.GetHashCode() + Version.GetHashCode();
            }
        }
    }
}
