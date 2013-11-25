using System;

namespace ENode.Messaging
{
    /// <summary>Represents a message payload.
    /// </summary>
    public interface IMessagePayload
    {
        /// <summary>Represents the unique identifier of the message payload.
        /// </summary>
        Guid Id { get; }
    }
}
