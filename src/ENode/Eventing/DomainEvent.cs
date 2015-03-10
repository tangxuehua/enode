using System;
using ENode.Domain;
using ENode.Infrastructure;

namespace ENode.Eventing
{
    /// <summary>Represents an abstract domain event.
    /// </summary>
    [Serializable]
    public abstract class DomainEvent<TAggregateRootId> : SequenceMessage<TAggregateRootId>, IDomainEvent
    {
        /// <summary>Default constructor.
        /// </summary>
        public DomainEvent() : base() { }
        /// <summary>Parameterized constructor.
        /// </summary>
        public DomainEvent(AggregateRoot<TAggregateRootId> aggregateRoot)
            : base(aggregateRoot.Id, ((IAggregateRoot)aggregateRoot).Version + 1)
        {
        }
    }
}
