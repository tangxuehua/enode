using System;
using ENode.Infrastructure;

namespace ENode.Eventing
{
    /// <summary>Represents an abstract generic domain event.
    /// </summary>
    [Serializable]
    public abstract class DomainEvent<TAggregateRootId> : SequenceMessage<TAggregateRootId>, IDomainEvent<TAggregateRootId>
    {
    }
}
