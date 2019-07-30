using ENode.Infrastructure;

namespace ENode.Eventing
{
    public class ProcessingDomainEventStreamMessage
    {
        private IMessageProcessContext _processContext;

        public ProcessingDomainEventStreamMessageMailBox MailBox { get; set; }
        public DomainEventStreamMessage Message { get; private set; }

        public ProcessingDomainEventStreamMessage(DomainEventStreamMessage message, IMessageProcessContext processContext)
        {
            Message = message;
            _processContext = processContext;
        }

        public void Complete()
        {
            _processContext.NotifyMessageProcessed();
            if (MailBox != null)
            {
                MailBox.CompleteRun();
            }
        }
    }
}
