using System;

namespace ENode.Messaging
{
    /// <summary>Represents a message.
    /// </summary>
    public interface IMessage
    {
        /// <summary>Represents the unique identifier for the message.
        /// </summary>
        Guid Id { get; }
    }
}
