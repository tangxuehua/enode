using System;
using ENode.Infrastructure;

namespace ENode.Eventing
{
    /// <summary>Represents an abstract generic domain event.
    /// </summary>
    [Serializable]
    public abstract class DomainEvent<TAggregateRootId> : Message, IDomainEvent<TAggregateRootId>
    {
        public string CommandId { get; set; }
        public TAggregateRootId AggregateRootId { get; set; }
        public string AggregateRootStringId { get; set; }
        public string AggregateRootTypeName { get; set; }
        public int Version { get; set; }
        public int Sequence { get; set; }
    }
}
