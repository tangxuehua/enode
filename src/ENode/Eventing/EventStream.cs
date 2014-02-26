using System;
using System.Collections.Generic;
using System.Linq;
using ENode.Infrastructure;

namespace ENode.Eventing
{
    /// <summary>Represents a stream of domain event.
    /// <remarks>
    /// One stream may contains several domain events, but they must belong to a single aggregate.
    /// </remarks>
    /// </summary>
    [Serializable]
    public class EventStream
    {
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="commandId"></param>
        /// <param name="aggregateRootId"></param>
        /// <param name="aggregateRootName"></param>
        /// <param name="version"></param>
        /// <param name="timestamp"></param>
        /// <param name="events"></param>
        public EventStream(Guid commandId, string aggregateRootId, string aggregateRootName, long version, DateTime timestamp, IEnumerable<IDomainEvent> events)
        {
            CommandId = commandId;
            AggregateRootId = aggregateRootId;
            AggregateRootName = aggregateRootName;
            Version = version;
            Timestamp = timestamp;
            VerifyEvents(events);
            Events = events;
        }

        /// <summary>The commandId which generate this event stream.
        /// </summary>
        public Guid CommandId { get; private set; }
        /// <summary>The aggregate root id.
        /// </summary>
        public string AggregateRootId { get; private set; }
        /// <summary>The aggregate root name.
        /// </summary>
        public string AggregateRootName { get; private set; }
        /// <summary>The version of the event stream.
        /// </summary>
        public long Version { get; private set; }
        /// <summary>The occurred time of the event stream.
        /// </summary>
        public DateTime Timestamp { get; private set; }
        /// <summary>The domain events of the event stream.
        /// </summary>
        public IEnumerable<IDomainEvent> Events { get; private set; }

        /// <summary>Overrides to return the whole event stream information.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var format = "[CommandId={0},AggregateRootName={1},AggregateRootId={2},Version={3},Timestamp={4},Events={5}]";
            return string.Format(format, CommandId, AggregateRootName, AggregateRootId, Version, Timestamp, string.Join("|", Events.Select(x => x.GetType().Name)));
        }

        private void VerifyEvents(IEnumerable<IDomainEvent> events)
        {
            foreach (var evnt in events)
            {
                if (evnt.AggregateRootId != AggregateRootId)
                {
                    throw new ENodeException("Domain event aggregate root Id mismatch, current domain event aggregateRootId:{0}, expected aggregateRootId:{1}", evnt.AggregateRootId, AggregateRootId);
                }
                if (evnt.Version != Version)
                {
                    throw new ENodeException("Domain event version mismatch, current domain event version:{0}, expected version:{1}", evnt.Version, Version);
                }
            }
        }
    }
}
