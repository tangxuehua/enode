using System;

namespace ENode.Eventing
{
    [Serializable]
    public class SourcingEvent<TAggregateRootId> : DomainEvent<TAggregateRootId>, ISourcingEvent
    {
        /// <summary>Parameterized constructor.
        /// </summary>
        public SourcingEvent(TAggregateRootId aggregateRootId) : base(aggregateRootId) { }
    }
}
