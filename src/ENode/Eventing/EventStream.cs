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
    public class EventStream : Message
    {
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        /// <param name="aggregateRootName"></param>
        /// <param name="version"></param>
        /// <param name="commandId"></param>
        /// <param name="timestamp"></param>
        /// <param name="events"></param>
        public EventStream(string aggregateRootId, string aggregateRootName, long version, Guid commandId, DateTime timestamp, IEnumerable<IEvent> events)
            : this(Guid.NewGuid(), aggregateRootId, aggregateRootName, version, commandId, timestamp, events)
        {
        }
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="aggregateRootId"></param>
        /// <param name="aggregateRootName"></param>
        /// <param name="version"></param>
        /// <param name="commandId"></param>
        /// <param name="timestamp"></param>
        /// <param name="events"></param>
        public EventStream(Guid id, string aggregateRootId, string aggregateRootName, long version, Guid commandId, DateTime timestamp, IEnumerable<IEvent> events)
            : base(id)
        {
            AggregateRootId = aggregateRootId;
            AggregateRootName = aggregateRootName;
            CommandId = commandId;
            Version = version;
            Timestamp = timestamp;
            Events = events ?? new List<IEvent>();
        }

        /// <summary>The aggregate root id.
        /// </summary>
        public string AggregateRootId { get; private set; }
        /// <summary>The aggregate root name.
        /// </summary>
        public string AggregateRootName { get; private set; }
        /// <summary>The command id.
        /// </summary>
        public Guid CommandId { get; private set; }
        /// <summary>The version of the event stream.
        /// </summary>
        public long Version { get; private set; }
        /// <summary>The occurred time of the event stream.
        /// </summary>
        public DateTime Timestamp { get; private set; }
        /// <summary>The domain events of the event stream.
        /// </summary>
        public IEnumerable<IEvent> Events { get; private set; }

        /// <summary>Check if a given type of domain event exist in the current event stream.
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <returns></returns>
        public bool HasEvent<TEvent>() where TEvent : class, IEvent
        {
            return Events.Any(x => x.GetType() == typeof(TEvent));
        }
        /// <summary>Find a domain event with the given event type from the current event stream.
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <returns></returns>
        public TEvent FindEvent<TEvent>() where TEvent : class, IEvent
        {
            return Events.SingleOrDefault(x => x.GetType() == typeof(TEvent)) as TEvent;
        }
        /// <summary>Get all the event type names, sperated by | character.
        /// </summary>
        /// <returns></returns>
        public string GetEventInformation()
        {
            return string.Join("|", Events.Select(x => x.GetType().Name));
        }
        /// <summary>Get the whole event stream string information.
        /// </summary>
        /// <returns></returns>
        public string GetStreamInformation()
        {
            var items = new List<object>
            {
                Id,
                AggregateRootName,
                AggregateRootId,
                Version,
                string.Join("-", Events.Select(x => x.GetType().Name)),
                CommandId,
                Timestamp
            };
            return string.Join("|", items);
        }
        /// <summary>Overrides to return the whole event stream information.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return GetEventInformation();
        }
    }
}
