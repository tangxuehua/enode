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
        /// <param name="message"></param>
        public void AddMessage(IMessage message) { }
        /// <summary>Remove a existing message from the queue.
        /// </summary>
        /// <param name="message"></param>
        public void RemoveMessage(IMessage message) { }
        /// <summary>Get all the existing messages of the queue.
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns></returns>
        public IEnumerable<IMessage> GetMessages(string queueName) { return new IMessage[] { }; }
    }
}
