using System;
using System.Collections.Generic;
using System.Linq;
using ENode.Messaging;

namespace ENode.Eventing
{
    /// <summary>Represents a stream of domain event.
    /// <remarks>
    /// One stream may contains several domain events, but they must belong to a single aggregate.
    /// </remarks>
    /// </summary>
    [Serializable]
    public class EventStream : Message, IMessage
    {
        public EventStream(string aggregateRootId, string aggregateRootName, long version, Guid commandId, DateTime timestamp, IEnumerable<IEvent> events)
            : this(Guid.NewGuid(), aggregateRootId, aggregateRootName, version, commandId, timestamp, events)
        {
        }
        public EventStream(Guid id, string aggregateRootId, string aggregateRootName, long version, Guid commandId, DateTime timestamp, IEnumerable<IEvent> events)
            : base(id)
        {
            this.AggregateRootId = aggregateRootId;
            this.AggregateRootName = aggregateRootName;
            this.CommandId = commandId;
            this.Version = version;
            this.Timestamp = timestamp;
            this.Events = events ?? new List<IEvent>();
        }

        public string AggregateRootId { get; private set; }
        public string AggregateRootName { get; private set; }
        public Guid CommandId { get; private set; }
        public long Version { get; private set; }
        public DateTime Timestamp { get; private set; }
        public IEnumerable<IEvent> Events { get; private set; }

        public bool HasEvent<TEvent>() where TEvent : class, IEvent
        {
            return Events.Any(x => x.GetType() == typeof(TEvent));
        }
        public TEvent FindEvent<TEvent>() where TEvent : class, IEvent
        {
            return Events.SingleOrDefault(x => x.GetType() == typeof(TEvent)) as TEvent;
        }
        public string GetEventInformation()
        {
            return string.Join("|", Events.Select(x => x.GetType().Name));
        }
        public string GetStreamInformation()
        {
            var items = new List<object>();
            items.Add(Id);
            items.Add(AggregateRootName);
            items.Add(AggregateRootId);
            items.Add(Version);
            items.Add(string.Join("-", Events.Select(x => x.GetType().Name)));
            items.Add(CommandId);
            items.Add(Timestamp);
            return string.Join("|", items);
        }
        public override string ToString()
        {
            return GetEventInformation();
        }
    }
}
