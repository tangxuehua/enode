namespace ENode.Infrastructure
{
    public class ProcessingApplicationMessage : IProcessingMessage<ProcessingApplicationMessage, IApplicationMessage>
    {
        private ProcessingMessageMailbox<ProcessingApplicationMessage, IApplicationMessage> _mailbox;
        private IMessageProcessContext _processContext;

        public IApplicationMessage Message { get; private set; }

        public ProcessingApplicationMessage(IApplicationMessage message, IMessageProcessContext processContext)
        {
            Message = message;
            _processContext = processContext;
        }

        public void SetMailbox(ProcessingMessageMailbox<ProcessingApplicationMessage, IApplicationMessage> mailbox)
        {
            _mailbox = mailbox;
        }
        public void Complete()
        {
            _processContext.NotifyMessageProcessed();
            if (_mailbox != null)
            {
                _mailbox.CompleteMessage(this);
            }
        }
    }
}
