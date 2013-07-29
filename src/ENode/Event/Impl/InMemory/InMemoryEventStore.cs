using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ENode.Infrastructure;

namespace ENode.Eventing
{
    public class InMemoryEventStore : IEventStore
    {
        private ConcurrentDictionary<EventKey, EventStream> _eventDict = new ConcurrentDictionary<EventKey, EventStream>();

        public void Append(EventStream stream)
        {
            if (stream != null)
            {
                if (!_eventDict.TryAdd(new EventKey(stream.AggregateRootId, stream.Version), stream))
                {
                    throw new ConcurrentException("");
                }
            }
        }
        public IEnumerable<EventStream> Query(string aggregateRootId, Type aggregateRootType, long minStreamVersion, long maxStreamVersion)
        {
            return _eventDict
                .Values
                .Where(x => x.AggregateRootId == aggregateRootId && x.Version >= minStreamVersion && x.Version <= maxStreamVersion)
                .OrderBy(x => x.Version);
        }
        public bool IsEventStreamExist(string aggregateRootId, Type aggregateRootType, Guid id)
        {
            return _eventDict.Values.Any(x => x.AggregateRootId == aggregateRootId && x.Id == id);
        }
        public IEnumerable<EventStream> QueryAll()
        {
            return _eventDict.Values;
        }

        class EventKey
        {
            public string AggregateRootId { get; private set; }
            public long StreamVersion { get; private set; }

            public EventKey(string aggregateRootId, long streamVersion)
            {
                AggregateRootId = aggregateRootId;
                StreamVersion = streamVersion;
            }

            public override bool Equals(object obj)
            {
                var eventKey = obj as EventKey;

                if (eventKey == null)
                {
                    return false;
                }
                else if (eventKey == this)
                {
                    return true;
                }

                return AggregateRootId == eventKey.AggregateRootId && StreamVersion == eventKey.StreamVersion;
            }
            public override int GetHashCode()
            {
                return AggregateRootId.GetHashCode() + StreamVersion.GetHashCode();
            }
        }
    }
}
