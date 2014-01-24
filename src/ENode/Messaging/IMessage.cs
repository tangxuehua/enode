using System;

namespace ENode.Messaging
{
    /// <summary>Represents a message.
    /// </summary>
    public interface IMessage
    {
        /// <summary>Represents the unique identifier of the message.
        /// </summary>
        Guid Id { get; }
        /// <summary>Represents the payload object of the message.
        /// </summary>
        object Payload { get; }
        /// <summary>Represents which queue the message from.
        /// </summary>
        string QueueName { get; }
        /// <summary>An event which will be raised when the message was handled.
        /// </summary>
        event Action<Guid, object> Handled;
    }
}
