using System;

namespace ENode.Infrastructure
{
    public class ProcessingApplicationMessage : IProcessingMessage<ProcessingApplicationMessage, IApplicationMessage, bool>
    {
        private ProcessingMessageMailbox<ProcessingApplicationMessage, IApplicationMessage, bool> _mailbox;
        private IMessageProcessContext _processContext;

        public IApplicationMessage Message { get; private set; }

        public ProcessingApplicationMessage(IApplicationMessage message, IMessageProcessContext processContext)
        {
            Message = message;
            _processContext = processContext;
        }

        public void SetMailbox(ProcessingMessageMailbox<ProcessingApplicationMessage, IApplicationMessage, bool> mailbox)
        {
            _mailbox = mailbox;
        }
        public void HandleLater()
        {
            _mailbox.AddWaitingForRetryMessage(this);
        }
        public void Complete(bool result)
        {
            _processContext.NotifyMessageProcessed();
            if (_mailbox != null)
            {
                _mailbox.CompleteMessage(this);
            }
        }
    }
}
