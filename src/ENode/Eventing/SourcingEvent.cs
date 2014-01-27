namespace ENode.Eventing
{
    public class SourcingEvent<TAggregateRootId> : DomainEvent<TAggregateRootId>, ISourcingEvent
    {
        /// <summary>Parameterized constructor.
        /// </summary>
        public SourcingEvent(TAggregateRootId aggregateRootId) : base(aggregateRootId) { }
    }
}
