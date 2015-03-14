using System;
using ENode.Eventing;

namespace ENode.Infrastructure
{
    public class ProcessingDomainEventStreamMessage : IProcessingMessage<ProcessingDomainEventStreamMessage, DomainEventStreamMessage, bool>
    {
        private ProcessingMessageMailbox<ProcessingDomainEventStreamMessage, DomainEventStreamMessage, bool> _mailbox;
        private IMessageProcessContext _processContext;

        public DomainEventStreamMessage Message { get; private set; }

        public ProcessingDomainEventStreamMessage(DomainEventStreamMessage message, IMessageProcessContext processContext)
        {
            Message = message;
            _processContext = processContext;
        }

        public void SetMailbox(ProcessingMessageMailbox<ProcessingDomainEventStreamMessage, DomainEventStreamMessage, bool> mailbox)
        {
            _mailbox = mailbox;
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
