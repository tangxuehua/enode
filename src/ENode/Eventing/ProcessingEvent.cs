using ENode.Infrastructure;

namespace ENode.Eventing
{
    public class ProcessingEvent
    {
        private IEventProcessContext _processContext;

        public ProcessingEventMailBox MailBox { get; set; }
        public DomainEventStreamMessage Message { get; private set; }

        public ProcessingEvent(DomainEventStreamMessage message, IEventProcessContext processContext)
        {
            Message = message;
            _processContext = processContext;
        }

        public void Complete()
        {
            _processContext.NotifyEventProcessed();
            if (MailBox != null)
            {
                MailBox.CompleteRun();
            }
        }
    }
}
