using System;

namespace ENode.Eventing
{
    /// <summary>Represents a domain event base class.
    /// </summary>
    [Serializable]
    public class DomainEvent<TAggregateRootId> : IDomainEvent
    {
        private Guid _id;
        private object _aggregateRootId;

        /// <summary>Parameterized constructor.
        /// </summary>
        public DomainEvent(TAggregateRootId aggregateRootId)
        {
            if (aggregateRootId == null)
            {
                throw new ArgumentNullException("aggregateRootId");
            }
            _id = Guid.NewGuid();
            _aggregateRootId = aggregateRootId;
            SourceId = aggregateRootId;
        }

        /// <summary>Represents the unique identifier for the domain event.
        /// </summary>
        public Guid Id { get; private set; }
        /// <summary>Represents the unique id of the aggregate root which raised this domain event.
        /// </summary>
        public TAggregateRootId SourceId { get; private set; }

        Guid IDomainEvent.Id
        {
            get { return _id; }
        }
        object IDomainEvent.AggregateRootId
        {
            get { return _aggregateRootId; }
        }
    }
}
