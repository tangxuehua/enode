using System;

namespace ENode.Eventing
{
    /// <summary>Represents an abstract base domain event.
    /// </summary>
    [Serializable]
    public abstract class DomainEvent<TAggregateRootId> : Event, IDomainEvent
    {
        /// <summary>Represents the source aggregate root id of the domain event.
        /// </summary>
        public TAggregateRootId AggregateRootId { get; set; }
        /// <summary>Represents the version of the domain event.
        /// </summary>
        public int Version { get; set; }
        /// <summary>Represents the occurred time of the domain event.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>Default constructor.
        /// </summary>
        public DomainEvent() : base() { }
        /// <summary>Parameterized constructor.
        /// </summary>
        public DomainEvent(TAggregateRootId aggregateRootId) : base()
        {
            if (aggregateRootId == null)
            {
                throw new ArgumentNullException("aggregateRootId");
            }
            AggregateRootId = aggregateRootId;
        }

        string IDomainEvent.AggregateRootId
        {
            get
            {
                if (this.AggregateRootId != null)
                {
                    return this.AggregateRootId.ToString();
                }
                return null;
            }
        }
    }
}
