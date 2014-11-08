using System;

namespace ENode.Eventing
{
    /// <summary>Represents a domain event.
    /// </summary>
    public interface IDomainEvent : IEvent
    {
        /// <summary>Represents the source aggregate root id of the domain event.
        /// </summary>
        string AggregateRootId { get; }
        /// <summary>Represents the version of the domain event.
        /// </summary>
        int Version { get; set; }
        /// <summary>Represents the occurred time of the domain event.
        /// </summary>
        DateTime Timestamp { get; set; }
    }
}
