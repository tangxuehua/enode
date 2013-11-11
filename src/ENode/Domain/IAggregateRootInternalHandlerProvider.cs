using System;
using ENode.Eventing;

namespace ENode.Domain
{
    /// <summary>Defines a provider interface to provide the aggregate root internal handler.
    /// </summary>
    public interface IAggregateRootInternalHandlerProvider
    {
        /// <summary>Get the internal event handler within the aggregate.
        /// </summary>
        Action<IAggregateRoot, IDomainEvent> GetInternalEventHandler(Type aggregateRootType, Type eventType);
    }
}
