using System;

namespace ENode.Infrastructure
{
    /// <summary>Represents a message.
    /// </summary>
    public interface IMessage
    {
        /// <summary>Represents the unique identifier of the message.
        /// </summary>
        string Id { get; set; }
        /// <summary>Represents the timestamp of the message.
        /// </summary>
        DateTime Timestamp { get; set; }
    }
}
