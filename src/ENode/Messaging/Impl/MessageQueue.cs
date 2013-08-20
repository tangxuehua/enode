using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ENode.Infrastructure;
using ENode.Infrastructure.Concurrent;
using ENode.Infrastructure.Logging;

namespace ENode.Messaging.Impl
{
    /// <summary>The abstract base message queue implementation of IMessageQueue.
    /// </summary>
    /// <typeparam name="T">The type of the message.</typeparam>
    public abstract class MessageQueue<T> : IMessageQueue<T> where T : class, IMessage
    {
        #region Private Variables

        private readonly IMessageStore _messageStore;
        private readonly BlockingCollection<T> _queue = new BlockingCollection<T>(new ConcurrentQueue<T>());
        private readonly ReaderWriterLockSlim _enqueueLocker = new ReaderWriterLockSlim();
        private readonly ReaderWriterLockSlim _dequeueLocker = new ReaderWriterLockSlim();

        #endregion

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
            _messageStore = ObjectContainer.Resolve<IMessageStore>();
            Logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().Name);
        }

        /// <summary>Initialize the message queue.
        /// </summary>
        public void Initialize()
        {
            _messageStore.Initialize(Name);
            var messages = _messageStore.GetMessages<T>(Name).ToList();
            foreach (var message in messages)
            {
                message.MarkAsRestoreFromStorage();
                _queue.Add(message);
                Logger.InfoFormat("{0} recovered, id:{1}", message.ToString(), message.Id);
            }
            OnInitialized(messages);
        }
        /// <summary>Called after the messages were recovered from the message store.
        /// </summary>
        /// <param name="initialQueueMessages"></param>
        protected virtual void OnInitialized(IEnumerable<T> initialQueueMessages) { }
        /// <summary>Enqueue the given message to the message queue. First add the message to message store, second enqueue the message to memory queue.
        /// </summary>
        /// <param name="message">The message to enqueue.</param>
        public void Enqueue(T message)
        {
            _enqueueLocker.AtomWrite(() =>
            {
                _messageStore.AddMessage(Name, message);
                _queue.Add(message);
                if (Logger.IsDebugEnabled)
                {
                    Logger.DebugFormat("{0} enqueued, id:{1}", message.ToString(), message.Id);
                }
            });
        }
        /// <summary>Dequeue the message from memory queue.
        /// </summary>
        /// <returns></returns>
        public T Dequeue()
        {
            return _queue.Take();
        }
        /// <summary>Remove the message from message store.
        /// </summary>
        /// <param name="message"></param>
        public void Complete(T message)
        {
            _dequeueLocker.AtomWrite(() => _messageStore.RemoveMessage(Name, message));
        }
    }
}
