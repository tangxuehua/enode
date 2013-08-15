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
        void AddMessage(string queueName, IMessage message);
        /// <summary>Remove a existing message from the queue.
        /// </summary>
        /// <param name="queueName">The name of the queue.</param>
        /// <param name="message">The message object.</param>
        void RemoveMessage(string queueName, IMessage message);
        /// <summary>Get all the existing messages of the queue.
        /// </summary>
        /// <typeparam name="T">The type of the message.</typeparam>
        /// <param name="queueName">The name of the queue.</param>
        /// <returns>Returns all the existing messages.</returns>
        IEnumerable<T> GetMessages<T>(string queueName) where T : class, IMessage;
    }
}
