using System;
using System.Collections.Generic;

namespace ENode.Eventing
{
    public interface IDomainEventStream : IEventStream
    {
        string AggregateRootId { get; }
        int AggregateRootTypeCode { get; }
        IEnumerable<IDomainEvent> DomainEvents { get; }
        int Version { get; }
        DateTime Timestamp { get; }
    }
}
