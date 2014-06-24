using ENode.Commanding;
using ENode.Domain;
using ENode.Infrastructure;

namespace ENode.Eventing
{
    public class EventCommittingContext
    {
        public IAggregateRoot AggregateRoot { get; private set; }
        public EventStream EventStream { get; private set; }
        public ProcessingCommand ProcessingCommand { get; private set; }
        public EventAppendResult EventAppendResult { get; set; }
        public ENodeException Exception { get; set; }

        public EventCommittingContext(IAggregateRoot aggregateRoot, EventStream eventStream, ProcessingCommand processingCommand)
        {
            AggregateRoot = aggregateRoot;
            EventStream = eventStream;
            ProcessingCommand = processingCommand;
        }
    }
}
