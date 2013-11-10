using System;

namespace ENode.Eventing
{
    /// <summary>Represents a base domain event.
    /// </summary>
    [Serializable]
    public class Event<TAggregateRootId> : IEvent
    {
        private Guid _id;
        private object _aggregateRootId;

        /// <summary>Parameterized constructor.
        /// </summary>
        public Event(TAggregateRootId aggregateRootId)
        {
            if (aggregateRootId == null)
            {
                throw new ArgumentNullException("aggregateRootId");
            }
            _aggregateRootId = aggregateRootId;
            _id = Guid.NewGuid();
            SourceId = aggregateRootId;
        }

        /// <summary>Represents the unique identifier for the domain event.
        /// </summary>
        public Guid Id { get; private set; }
        /// <summary>Represents the unique id of the aggregate root which raised this domain event.
        /// </summary>
        public TAggregateRootId SourceId { get; private set; }

        Guid IEvent.Id
        {
            get { return _id; }
        }
        object IEvent.AggregateRootId
        {
            get { return _aggregateRootId; }
        }
    }
}
