using System;

namespace ENode.Eventing
{
    /// <summary>Represents a source-able domain event base class.
    /// </summary>
    [Serializable]
    public class SourcableDomainEvent<TAggregateRootId> : DomainEvent<TAggregateRootId>, ISourcableEvent
    {
        public SourcableDomainEvent(TAggregateRootId aggregateRootId) : base(aggregateRootId) { }
    }
}
