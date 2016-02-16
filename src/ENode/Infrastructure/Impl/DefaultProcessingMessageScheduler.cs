using System.Threading.Tasks;

namespace ENode.Infrastructure
{
    public class DefaultProcessingMessageScheduler<X, Y, Z> : IProcessingMessageScheduler<X, Y, Z>
        where X : class, IProcessingMessage<X, Y, Z>
        where Y : IMessage
    {
        private readonly IProcessingMessageHandler<X, Y, Z> _messageHandler;

        public DefaultProcessingMessageScheduler(IProcessingMessageHandler<X, Y, Z> messageHandler)
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
        public void ScheduleMailbox(ProcessingMessageMailbox<X, Y, Z> mailbox)
        {
            if (mailbox.EnterHandlingMessage())
            {
                Task.Factory.StartNew(obj =>
                {
                    var currentMailbox = obj as ProcessingMessageMailbox<X, Y, Z>;
                    Task.Factory.StartNew(currentMailbox.Run);
                }, mailbox);
            }
        }
    }
}
