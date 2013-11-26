using System;

namespace ENode.Messaging
{
    /// <summary>Represents a message.
    /// </summary>
    public interface IMessage : IPayload
    {
        /// <summary>Represents the payload object of the message.
        /// </summary>
        object Payload { get; }
        /// <summary>Represents which queue the message from.
        /// </summary>
        string QueueName { get; }
    }
}
