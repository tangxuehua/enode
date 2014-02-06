using ENode.Commanding;
using ENode.Domain;
using ENode.Infrastructure;

namespace ENode.Eventing
{
    public class EventProcessingContext
    {
        public IAggregateRoot AggregateRoot { get; private set; }
        public EventStream EventStream { get; private set; }
        public ProcessingCommand ProcessingCommand { get; private set; }
        public ENodeException Exception { get; set; }
        public EventCommitStatus CommitStatus { get; set; }

        public EventProcessingContext(IAggregateRoot aggregateRoot, EventStream eventStream, ProcessingCommand processingCommand)
        {
            AggregateRoot = aggregateRoot;
            EventStream = eventStream;
            ProcessingCommand = processingCommand;
        }
    }
}
