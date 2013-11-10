using System;

namespace ENode.Eventing
{
    /// <summary>Represents a domain event interface.
    /// </summary>
    public interface IEvent
    {
        /// <summary>Represents the unique identifier of the domain event.
        /// </summary>
        Guid Id { get; }
        /// <summary>Represents the unique id of the aggregate root which raised this domain event.
        /// </summary>
        object AggregateRootId { get; }
    }
}
