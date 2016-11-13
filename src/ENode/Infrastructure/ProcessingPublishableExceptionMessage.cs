using System;

namespace ENode.Infrastructure
{
    public class ProcessingPublishableExceptionMessage : IProcessingMessage<ProcessingPublishableExceptionMessage, IPublishableException>
    {
        private ProcessingMessageMailbox<ProcessingPublishableExceptionMessage, IPublishableException> _mailbox;
        private IMessageProcessContext _processContext;

        public IPublishableException Message { get; private set; }

        public ProcessingPublishableExceptionMessage(IPublishableException message, IMessageProcessContext processContext)
        {
            Message = message;
            _processContext = processContext;
        }

        public void SetMailbox(ProcessingMessageMailbox<ProcessingPublishableExceptionMessage, IPublishableException> mailbox)
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
