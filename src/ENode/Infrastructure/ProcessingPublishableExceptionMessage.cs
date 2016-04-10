using System;

namespace ENode.Infrastructure
{
    public class ProcessingPublishableExceptionMessage : IProcessingMessage<ProcessingPublishableExceptionMessage, IPublishableException, bool>
    {
        private ProcessingMessageMailbox<ProcessingPublishableExceptionMessage, IPublishableException, bool> _mailbox;
        private IMessageProcessContext _processContext;

        public IPublishableException Message { get; private set; }

        public ProcessingPublishableExceptionMessage(IPublishableException message, IMessageProcessContext processContext)
        {
            Message = message;
            _processContext = processContext;
        }

        public void SetMailbox(ProcessingMessageMailbox<ProcessingPublishableExceptionMessage, IPublishableException, bool> mailbox)
        {
            _mailbox = mailbox;
        }
        public void SetResult(bool result)
        {
            _processContext.NotifyMessageProcessed();
            if (_mailbox != null)
            {
                _mailbox.CompleteMessage(this);
            }
        }
    }
}
