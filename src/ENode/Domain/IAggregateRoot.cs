using System;
using System.Collections.Generic;
using ENode.Eventing;

namespace ENode.Domain
{
    /// <summary>Represents an aggregate root.
    /// </summary>
    public interface IAggregateRoot
    {
        string UniqueId { get; }
        long Version { get; }
        IEnumerable<IDomainEvent> GetUncommittedEvents();
        void ClearUncommittedEvents();
    }
}
