using System;
using System.Threading.Tasks;
using ECommon.Scheduling;
using ECommon.Utilities;

namespace ENode.Infrastructure
{
    public class DefaultProcessingMessageScheduler<X, Y, Z> : IProcessingMessageScheduler<X, Y, Z>
        where X : class, IProcessingMessage<X, Y, Z>
        where Y : IMessage
    {
        private TaskFactory _taskFactory;
        private readonly IProcessingMessageHandler<X, Y, Z> _messageHandler;

        public DefaultProcessingMessageScheduler(IProcessingMessageHandler<X, Y, Z> messageHandler)
        {
            _messageHandler = messageHandler;
            _taskFactory = new TaskFactory(new LimitedConcurrencyLevelTaskScheduler(Environment.ProcessorCount));
        }

        public void SetConcurrencyLevel(int concurrentLevel)
        {
            Ensure.Positive(concurrentLevel, "concurrentLevel");
            _taskFactory = new TaskFactory(new LimitedConcurrencyLevelTaskScheduler(concurrentLevel));
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
