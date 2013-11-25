using System.Collections.Generic;

namespace ENode.Messaging
{
    /// <summary>Represents a message store which used to persist queue messages.
    /// </summary>
    public interface IMessageStore
    {
        /// <summary>Initialize the given message queue.
        /// </summary>
        /// <param name="queueName">The name of the queue.</param>
        void Initialize(string queueName);
        /// <summary>Persist a new message to the queue.
        /// </summary>
        /// <param name="queueName">The name of the queue.</param>
        /// <param name="message">The message object.</param>
        void AddMessage(IMessage message);
        /// <summary>Remove a existing message from the queue.
        /// </summary>
        /// <param name="queueName">The name of the queue.</param>
        /// <param name="message">The message object.</param>
        void RemoveMessage(IMessage message);
        /// <summary>Get all the existing messages of the queue.
        /// </summary>
        /// <param name="queueName">The name of the queue.</param>
        /// <returns>Returns all the existing messages.</returns>
        IEnumerable<IMessage> GetMessages(string queueName);
    }
}
