using System;
using System.Collections.Concurrent;
using ENode.Infrastructure;
using ENode.Infrastructure.Logging;

namespace ENode.Messaging.Impl
{
    /// <summary>The abstract implementation of IMessageQueue.
    /// </summary>
    /// <typeparam name="TMessagePayload">The type of the message payload.</typeparam>
    public abstract class MessageQueue<TMessagePayload> : IMessageQueue<TMessagePayload> where TMessagePayload : class, IPayload
    {
        private readonly BlockingCollection<Message<TMessagePayload>> _queue = new BlockingCollection<Message<TMessagePayload>>(new ConcurrentQueue<Message<TMessagePayload>>());

        /// <summary>The name of the queue.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>The logger which maybe used by the message queue.
        /// </summary>
        protected ILogger Logger { get; private set; }

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="name">The name of the queue.</param>
        /// <exception cref="ArgumentNullException">Throw when the queue name is null or empty.</exception>
        protected MessageQueue(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            Name = name;
            Logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().Name);
        }

        /// <summary>Enqueue a message to the queue.
        /// </summary>
        /// <param name="message">The message to enqueue.</param>
        public void Enqueue(Message<TMessagePayload> message)
        {
            _queue.Add(message);
        }
        /// <summary>Dequeue the top message from the queue. If no message exist, block the current thread.
        /// </summary>
        /// <returns></returns>
        public Message<TMessagePayload> Dequeue()
        {
            return _queue.Take();
        }
    }
}
