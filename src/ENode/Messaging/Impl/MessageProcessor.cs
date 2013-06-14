using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using ENode.Infrastructure;

namespace ENode.Messaging
{
    public abstract class Processor<TQueue, TMessageExecutor, TMessage> : IProcessor<TQueue, TMessage>
        where TQueue : IQueue<TMessage>
        where TMessageExecutor : class, IMessageExecutor<TMessage>
        where TMessage : class, IMessage
    {
        private IList<TMessageExecutor> _messageExecutors;
        private IList<Worker> _workers;
        private TQueue _bindingQueue;
        private BlockingCollection<TMessage> _retryQueue;
        private ILogger _logger;
        private bool _started;

        public TQueue BindingQueue
        {
            get { return _bindingQueue; }
        }

        public Processor(TQueue bindingQueue, int messageExecutorCount)
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
            _retryQueue = new BlockingCollection<TMessage>(new ConcurrentQueue<TMessage>());

            _messageExecutors = new List<TMessageExecutor>();
            _workers = new List<Worker>();

            for (var index = 0; index < messageExecutorCount; index++)
            {
                var messageExecutor = ObjectContainer.Resolve<TMessageExecutor>();
                _messageExecutors.Add(messageExecutor);
                _workers.Add(new Worker(() => ProcessMessage(messageExecutor)));
            }

            var retryMessageExecutor = ObjectContainer.Resolve<TMessageExecutor>();
            _messageExecutors.Add(retryMessageExecutor);
            _workers.Add(new Worker(() => RetryMessage(retryMessageExecutor)));

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
            _logger.InfoFormat("Processor started, binding queue {0}, worker count:{1}.", _bindingQueue.Name, _workers.Count - 1);
        }

        private void ProcessMessage(TMessageExecutor messageExecutor)
        {
            var message = _bindingQueue.Dequeue();
            if (message != null)
            {
                ProcessMessageRecursively(messageExecutor, message, 0, 3);
            }
        }
        private void ProcessMessageRecursively(TMessageExecutor messageExecutor, TMessage message, int retriedCount, int maxRetryCount)
        {
            var success = ExecuteMessage(messageExecutor, message);

            if (success)
            {
                _bindingQueue.Complete(message);
            }
            else if (retriedCount < maxRetryCount)
            {
                _logger.InfoFormat("Retring to handling message:{0} for {1} times.", message.ToString(), retriedCount + 1);
                ProcessMessageRecursively(messageExecutor, message, retriedCount + 1, maxRetryCount);
            }
            else
            {
                _retryQueue.Add(message);
            }
        }
        private void RetryMessage(TMessageExecutor messageExecutor)
        {
            var message = _retryQueue.Take();
            if (message != null)
            {
                //Sleep 5 senconds to prevent CPU busy if execute message always not success.
                Thread.Sleep(5000);

                var success = ExecuteMessage(messageExecutor, message);
                if (success)
                {
                    _bindingQueue.Complete(message);
                }
                else
                {
                    _retryQueue.Add(message);
                }
            }
        }
        private bool ExecuteMessage(TMessageExecutor messageExecutor, TMessage message)
        {
            var success = false;
            try
            {
                success = messageExecutor.Execute(message);
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Unknown exception raised when handling message:{0}.", message.ToString()), ex);
            }
            return success;
        }
    }
}
