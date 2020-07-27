using ENode.Commanding;
using ENode.Domain;

namespace ENode.Eventing
{
    public class EventCommittingContext
    {
        public EventCommittingContextMailBox MailBox { get; set; }
        public DomainEventStream EventStream { get; private set; }
        public ProcessingCommand ProcessingCommand { get; private set; }

        public EventCommittingContext(DomainEventStream eventStream, ProcessingCommand processingCommand)
        {
            EventStream = eventStream;
            ProcessingCommand = processingCommand;
        }
    }
}
