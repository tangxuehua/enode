using System.Threading.Tasks;
using ECommon.Scheduling;
using ENode.Configurations;

namespace ENode.Infrastructure
{
    public class DefaultProcessingMessageScheduler<X, Y, Z> : IProcessingMessageScheduler<X, Y, Z> where X : class, IProcessingMessage<X, Y, Z>
    {
        private readonly TaskFactory _taskFactory;
        private readonly IProcessingMessageHandler<X, Y, Z> _messageHandler;

        public DefaultProcessingMessageScheduler(IProcessingMessageHandler<X, Y, Z> messageHandler)
        {
            var setting = ENodeConfiguration.Instance.Setting;
            //TODO set maxDegreeOfParallelism
            _taskFactory = new TaskFactory(new LimitedConcurrencyLevelTaskScheduler(4));
            _messageHandler = messageHandler;
        }

        public void ScheduleMessage(X processingMessage)
        {
            _taskFactory.StartNew(obj =>
            {
                _messageHandler.HandleAsync((X)obj);
            }, processingMessage);
        }
        public void ScheduleMailbox(ProcessingMessageMailbox<X, Y, Z> mailbox)
        {
            _taskFactory.StartNew(obj =>
            {
                var currentMailbox = obj as ProcessingMessageMailbox<X, Y, Z>;
                if (currentMailbox.EnterHandlingMessage())
                {
                    _taskFactory.StartNew(currentMailbox.Run);
                }
            }, mailbox);
        }
    }
}
