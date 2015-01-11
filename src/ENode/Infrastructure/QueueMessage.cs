using ECommon.Utilities;

namespace ENode.Infrastructure
{
    public class QueueMessage<T> where T : class
    {
        public object HashKey { get; private set; }
        public T Payload { get; private set; }
        public int RetryTimes { get; private set; }
        public IProcessContext<T> ProcessContext { get; private set; }

        public QueueMessage(object hashKey, T payload, IProcessContext<T> processContext)
        {
            Ensure.NotNull(hashKey, "hashKey");
            Ensure.NotNull(payload, "payload");
            Ensure.NotNull(processContext, "processContext");
            HashKey = hashKey;
            Payload = payload;
            ProcessContext = processContext;
        }

        public void IncreaseRetryTimes()
        {
            RetryTimes++;
        }
        public void Complete()
        {
            ProcessContext.OnProcessed(Payload);
        }
    }
}
