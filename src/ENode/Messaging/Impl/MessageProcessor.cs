using System;
using System.Collections.Generic;
using ENode.Infrastructure;

namespace ENode.Messaging
{
    public abstract class MessageProcessor<TQueue, TMessageExecutor, TMessage> : IMessageProcessor<TQueue, TMessage>
        where TQueue : IMessageQueue<TMessage>
        where TMessageExecutor : class, IMessageExecutor<TMessage>
        where TMessage : class, IMessage
    {
        private IList<TMessageExecutor> _messageExecutors;
        private IList<Worker> _workers;
        private TQueue _bindingQueue;
        private ILogger _logger;
        private bool _started;

        public TQueue BindingQueue
        {
            get { return _bindingQueue; }
        }

        public MessageProcessor(TQueue bindingQueue, int messageExecutorCount)
        {
            if (bindingQueue == null)
            {
                throw new ArgumentNullException("bindingQueue");
            }
            if (messageExecutorCount <= 0)
            {
                throw new Exception(string.Format("There must at least one message executor for {0}.", GetType().Name));
            }

            _bindingQueue = bindingQueue;
            _messageExecutors = new List<TMessageExecutor>();
            _workers = new List<Worker>();

            for (var index = 0; index < messageExecutorCount; index++)
            {
                var messageExecutor = ObjectContainer.Resolve<TMessageExecutor>();
                _messageExecutors.Add(messageExecutor);
                _workers.Add(new Worker(() => ProcessMessage(messageExecutor)));
            }

            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().Name);
            _started = false;
        }

        public void Initialize()
        {
            _bindingQueue.Initialize();
        }
        public void Start()
        {
            if (_started) return;

            foreach (var worker in _workers)
            {
                worker.Start();
            }
            _started = true;
            _logger.InfoFormat("Processor started, binding queue {0}, worker count:{1}.", _bindingQueue.Name, _workers.Count);
        }

        private void ProcessMessage(TMessageExecutor messageExecutor)
        {
            var message = _bindingQueue.Dequeue();
            if (message != null)
            {
                try
                {
                    messageExecutor.Execute(message, _bindingQueue);
                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("Exception raised when handling queue message:{0}.", message.ToString()), ex);
                }
            }
        }
    }
}
