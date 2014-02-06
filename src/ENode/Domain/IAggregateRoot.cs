using System;
using System.Collections.Generic;
using ENode.Eventing;

namespace ENode.Domain
{
    /// <summary>Represents an aggregate root.
    /// </summary>
    public interface IAggregateRoot
    {
        object UniqueId { get; }
        long Version { get; }
        IEnumerable<IDomainEvent> GetUncommittedEvents();
        void ClearUncommittedEvents();
    }
}
