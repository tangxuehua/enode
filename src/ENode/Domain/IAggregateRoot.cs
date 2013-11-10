using System;
using System.Collections.Generic;
using ENode.Eventing;

namespace ENode.Domain
{
    /// <summary>Represents the aggregate root interface.
    /// </summary>
    public interface IAggregateRoot
    {
        object UniqueId { get; }
        long Version { get; }
        IEnumerable<IEvent> GetUncommittedEvents();
    }
}
