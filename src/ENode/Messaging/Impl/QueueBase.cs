using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using ENode.Infrastructure;

namespace ENode.Messaging
{
    public abstract class QueueBase<T> : IQueue<T> where T : class, IMessage
    {
        #region Private Variables

        private IMessageStore _messageStore;
        private BlockingCollection<T> _queue = new BlockingCollection<T>(new ConcurrentQueue<T>());
        private ReaderWriterLockSlim _enqueueLocker = new ReaderWriterLockSlim();
        private ReaderWriterLockSlim _dequeueLocker = new ReaderWriterLockSlim();

        #endregion

        public string Name { get; private set; }
        protected ILogger Logger { get; private set; }

        public QueueBase(string name)
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
                _queue.Add(message);
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
