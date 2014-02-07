using System;

namespace ENode.Eventing
{
    /// <summary>Represents a domain event.
    /// </summary>
    public interface IDomainEvent
    {
        /// <summary>Represents the unique identifier of the domain event.
        /// </summary>
        Guid Id { get; }
        /// <summary>Represents the unique id of the aggregate root which raised this domain event.
        /// </summary>
        string AggregateRootId { get; }
    }
}
