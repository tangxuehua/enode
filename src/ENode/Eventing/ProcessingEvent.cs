namespace ENode.Eventing
{
    public class ProcessingEvent
    {
        public IEventProcessContext ProcessContext { get; private set; }
        public ProcessingEventMailBox MailBox { get; set; }
        public DomainEventStreamMessage Message { get; private set; }

        public ProcessingEvent(DomainEventStreamMessage message, IEventProcessContext processContext)
        {
            Message = message;
            ProcessContext = processContext;
        }

        public void Complete()
        {
            ProcessContext.NotifyEventProcessed();
            if (MailBox != null)
            {
                MailBox.CompleteRun();
            }
        }
    }
}
