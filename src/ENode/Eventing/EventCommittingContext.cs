using ENode.Commanding;
using ENode.Domain;
using ENode.Eventing.Impl;

namespace ENode.Eventing
{
    public class EventCommittingContext
    {
        public IAggregateRoot AggregateRoot { get; private set; }
        public DomainEventStream EventStream { get; private set; }
        public ProcessingCommand ProcessingCommand { get; private set; }
        public EventMailBox EventMailBox { get; set; }
        public EventCommittingContext Next { get; set; }

        public EventCommittingContext(IAggregateRoot aggregateRoot, DomainEventStream eventStream, ProcessingCommand processingCommand)
        {
            AggregateRoot = aggregateRoot;
            EventStream = eventStream;
            ProcessingCommand = processingCommand;
        }
    }
}
