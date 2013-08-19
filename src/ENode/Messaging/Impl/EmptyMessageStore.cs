using System.Collections.Generic;

namespace ENode.Messaging.Impl
{
    /// <summary>A empty message store which always not store message, which only used when unit testing.
    /// </summary>
    public class EmptyMessageStore : IMessageStore
    {
        /// <summary>Initialize the message store.
        /// </summary>
        /// <param name="queueName"></param>
        public void Initialize(string queueName) { }
        /// <summary>Persist a new message to the queue.
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="message"></param>
        public void AddMessage(string queueName, IMessage message) { }
        /// <summary>Remove a existing message from the queue.
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="message"></param>
        public void RemoveMessage(string queueName, IMessage message) { }
        /// <summary>Get all the existing messages of the queue.
        /// </summary>
        /// <param name="queueName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IEnumerable<T> GetMessages<T>(string queueName) where T : class, IMessage { return new T[] { }; }
    }
}
