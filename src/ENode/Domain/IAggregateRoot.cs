using System;
using System.Collections.Generic;
using ENode.Eventing;

namespace ENode.Domain
{
    /// <summary>Represents an aggregate root interface.
    /// </summary>
    public interface IAggregateRoot
    {
        string UniqueId { get; set; }
        int Version { get; }
        IEnumerable<IDomainEvent> GetUncommittedEvents();
        void ClearUncommittedEvents();
    }
}
