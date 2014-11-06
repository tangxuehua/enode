using ENode.Commanding;
using ENode.Domain;

namespace ENode.Eventing
{
    public class EventCommittingContext
    {
        public IAggregateRoot AggregateRoot { get; private set; }
        public DomainEventStream EventStream { get; private set; }
        public ProcessingCommand ProcessingCommand { get; private set; }
        public EventAppendResult EventAppendResult { get; set; }

        public EventCommittingContext(IAggregateRoot aggregateRoot, DomainEventStream eventStream, ProcessingCommand processingCommand)
        {
            AggregateRoot = aggregateRoot;
            EventStream = eventStream;
            ProcessingCommand = processingCommand;
        }
    }
}
