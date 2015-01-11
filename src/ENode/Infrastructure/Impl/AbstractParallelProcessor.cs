namespace ENode.Infrastructure.Impl
{
    public abstract class AbstractParallelProcessor<T> : IProcessor<T> where T : class
    {
        private readonly ParallelProcessor<T> _parallelProcessor;

        public string Name { get; set; }

        public AbstractParallelProcessor(int parallelThreadCount, string handleMessageActionName)
        {
            Name = GetType().Name;
            _parallelProcessor = new ParallelProcessor<T>(parallelThreadCount, handleMessageActionName, HandleQueueMessage);
        }

        public void Start()
        {
            _parallelProcessor.Start();
        }
        public void Process(T message, IProcessContext<T> processContext)
        {
            _parallelProcessor.EnqueueMessage(CreateQueueMessage(message, processContext));
        }

        protected abstract QueueMessage<T> CreateQueueMessage(T message, IProcessContext<T> processContext);
        protected abstract void HandleQueueMessage(QueueMessage<T> queueMessage);
        protected void OnMessageHandled(bool success, QueueMessage<T> queueMessage)
        {
            if (success)
            {
                queueMessage.Complete();
            }
            else
            {
                queueMessage.IncreaseRetryTimes();
                _parallelProcessor.RetryMessage(queueMessage);
            }
        }
    }
}
