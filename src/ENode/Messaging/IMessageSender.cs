using System;

namespace ENode.Messaging
{
    /// <summary>Represents a sender to send a payload a to a specific IMessageQueue.
    /// </summary>
    public interface IMessageSender<TMessagePayload>
    {
        /// <summary>Send the given payload object to a specific message queue.
        /// </summary>
        void Send(TMessagePayload payload);
        /// <summary>Send the given payload object to a specific message queue.
        /// </summary>
        void Send(TMessagePayload payload, Guid messageId);
        /// <summary>Send the given payload object to a specific message queue.
        /// </summary>
        void Send(TMessagePayload payload, Guid messageId, Action<Guid, object> messageHandledCallback);
    }
}
