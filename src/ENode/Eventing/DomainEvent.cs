using System;

namespace ENode.Eventing
{
    /// <summary>Represents a domain event base class.
    /// </summary>
    [Serializable]
    public abstract class DomainEvent<TAggregateRootId> : IDomainEvent
    {
        /// <summary>Parameterized constructor.
        /// </summary>
        public DomainEvent(TAggregateRootId aggregateRootId)
        {
            if (aggregateRootId == null)
            {
                throw new ArgumentNullException("aggregateRootId");
            }
            Id = Guid.NewGuid();
            AggregateRootId = aggregateRootId;
        }

        /// <summary>Represents the unique id of the domain event.
        /// </summary>
        public Guid Id { get; private set; }
        /// <summary>Represents the unique id of the aggregate root which raised this domain event.
        /// </summary>
        public TAggregateRootId AggregateRootId { get; private set; }
        /// <summary>Represents the unique id of the aggregate root, this property is only used by framework.
        /// </summary>
        object IDomainEvent.AggregateRootId
        {
            get { return AggregateRootId; }
        }
    }
}
