using System;

namespace ENode.Eventing
{
    /// <summary>Represents a domain event.
    /// </summary>
    public interface IDomainEvent
    {
        /// <summary>Represents the unique identifier of the domain event.
        /// </summary>
        string Id { get; }
        /// <summary>Represents the unique id of the aggregate root which raised this domain event.
        /// </summary>
        string AggregateRootId { get; }
        /// <summary>Represents the version of the domain event.
        /// </summary>
        int Version { get; set; }
        /// <summary>Represents the time of when this domain event raised.
        /// </summary>
        DateTime Timestamp { get; set; }
    }
}
