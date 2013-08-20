using System;
using System.Collections.Generic;
using ENode.Infrastructure;
using ENode.Infrastructure.Logging;

namespace ENode.Messaging.Impl
{
    /// <summary>The abstract base message processor implementation of IMessageProcessor.
    /// </summary>
    /// <typeparam name="TQueue">The type of the message queue.</typeparam>
    /// <typeparam name="TMessageExecutor">The type of the message executor.</typeparam>
    /// <typeparam name="TMessage">The type of the message.</typeparam>
    public abstract class MessageProcessor<TQueue, TMessageExecutor, TMessage> : IMessageProcessor<TQueue, TMessage>
        where TQueue : class, IMessageQueue<TMessage>
        where TMessageExecutor : class, IMessageExecutor<TMessage>
        where TMessage : class, IMessage
    {
        private readonly IList<Worker> _workers;
        private readonly TQueue _bindingQueue;
        private readonly ILogger _logger;
        private bool _started;

        /// <summary>The binding queue of the message processor.
        /// </summary>
        public TQueue BindingQueue
        {
            get { return _bindingQueue; }
        }

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="bindingQueue"></param>
        /// <param name="messageExecutorCount"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        protected MessageProcessor(TQueue bindingQueue, int messageExecutorCount)
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
            IList<TMessageExecutor> messageExecutors = new List<TMessageExecutor>();
            _workers = new List<Worker>();

            for (var index = 0; index < messageExecutorCount; index++)
            {
                var messageExecutor = ObjectContainer.Resolve<TMessageExecutor>();
                messageExecutors.Add(messageExecutor);
                _workers.Add(new Worker(() => ProcessMessage(messageExecutor)));
            }

            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().Name);
            _started = false;
        }

        /// <summary>Initialize the message processor.
        /// </summary>
        public void Initialize()
        {
            _bindingQueue.Initialize();
        }
        /// <summary>Start the message processor.
        /// </summary>
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
            if (message == null) return;
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
