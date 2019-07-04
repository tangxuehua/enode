using ENode.Commanding;
using ENode.Domain;
using ENode.Infrastructure;

namespace ENode.Eventing
{
    public class EventCommittingContext : IMailBoxMessage<EventCommittingContext, bool>
    {
        public IMailBox<EventCommittingContext, bool> MailBox { get; set; }
        public long Sequence { get; set; }

        public IAggregateRoot AggregateRoot { get; private set; }
        public DomainEventStream EventStream { get; private set; }
        public ProcessingCommand ProcessingCommand { get; private set; }
        public EventCommittingContext Next { get; set; }

        public EventCommittingContext(IAggregateRoot aggregateRoot, DomainEventStream eventStream, ProcessingCommand processingCommand)
        {
            AggregateRoot = aggregateRoot;
            EventStream = eventStream;
            ProcessingCommand = processingCommand;
        }
    }
}
