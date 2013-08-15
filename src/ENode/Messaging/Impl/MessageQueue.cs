using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using ENode.Infrastructure;

namespace ENode.Messaging
{
    public abstract class MessageQueue<T> : IMessageQueue<T> where T : class, IMessage
    {
        #region Private Variables

        private IMessageStore _messageStore;
        private BlockingCollection<T> _queue = new BlockingCollection<T>(new ConcurrentQueue<T>());
        private ReaderWriterLockSlim _enqueueLocker = new ReaderWriterLockSlim();
        private ReaderWriterLockSlim _dequeueLocker = new ReaderWriterLockSlim();

        #endregion

        public string Name { get; private set; }
        protected ILogger Logger { get; private set; }

        public MessageQueue(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            Name = name;
            _messageStore = ObjectContainer.Resolve<IMessageStore>();
            Logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().Name);
        }

        public void Initialize()
        {
            _messageStore.Initialize(Name);
            var messages = _messageStore.GetMessages<T>(Name);
            foreach (var message in messages)
            {
                message.MarkAsRestoreFromStorage();
                _queue.Add(message);
                Logger.InfoFormat("{0} recovered, id:{1}", message.ToString(), message.Id);
            }
            OnInitialized(messages);
        }
        protected virtual void OnInitialized(IEnumerable<T> initialQueueMessages) { }

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
        public T Dequeue()
        {
            return _queue.Take();
        }
        public void Complete(T message)
        {
            _dequeueLocker.AtomWrite(() =>
            {
                _messageStore.RemoveMessage(Name, message);
            });
        }
    }
}
