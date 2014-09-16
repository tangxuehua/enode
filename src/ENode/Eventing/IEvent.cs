using System;

namespace ENode.Eventing
{
    /// <summary>Represents a domain event.
    /// </summary>
    public interface IEvent
    {
        /// <summary>Represents the unique identifier of the event.
        /// </summary>
        string Id { get; }
        /// <summary>Represents the time when the event happened.
        /// </summary>
        DateTime Timestamp { get; set; }
    }
}
