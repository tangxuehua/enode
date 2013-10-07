using System;

namespace ENode.Eventing
{
    /// <summary>Represents a domain event interface.
    /// </summary>
    public interface IEvent
    {
        /// <summary>Represents the unique identifier for the event.
        /// </summary>
        Guid Id { get; }
        /// <summary>Represents the source of the event, which means which aggregate raised this event.
        /// </summary>
        object SourceId { get; }
    }
}
