using System.Threading.Tasks;

namespace ENode.Infrastructure
{
    public class DefaultProcessingMessageScheduler<X, Y> : IProcessingMessageScheduler<X, Y>
        where X : class, IProcessingMessage<X, Y>
        where Y : IMessage
    {
        private readonly IProcessingMessageHandler<X, Y> _messageHandler;

        public DefaultProcessingMessageScheduler(IProcessingMessageHandler<X, Y> messageHandler)
        {
            _messageHandler = messageHandler;
        }

        public void ScheduleMessage(X processingMessage)
        {
            Task.Factory.StartNew(obj =>
            {
                _messageHandler.HandleAsync((X)obj);
            }, processingMessage);
        }
        public void ScheduleMailbox(ProcessingMessageMailbox<X, Y> mailbox)
        {
            Task.Factory.StartNew(mailbox.Run);
        }
    }
}
