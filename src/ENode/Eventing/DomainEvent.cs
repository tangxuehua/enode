using System;

namespace ENode.Eventing
{
    /// <summary>Represents a domain event base class.
    /// </summary>
    [Serializable]
    public class DomainEvent<TAggregateRootId> : IDomainEvent
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
            SourceId = aggregateRootId;
        }

        /// <summary>Represents the unique id of the domain event.
        /// </summary>
        public Guid Id { get; private set; }
        /// <summary>Represents the unique id of the aggregate root which raised this domain event.
        /// </summary>
        public TAggregateRootId SourceId { get; private set; }

        object IDomainEvent.AggregateRootId
        {
            get { return SourceId; }
        }
    }
}
