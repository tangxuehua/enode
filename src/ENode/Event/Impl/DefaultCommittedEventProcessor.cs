using ENode.Messaging;

namespace ENode.Eventing
{
    public class DefaultCommittedEventProcessor :
        MessageProcessor<ICommittedEventQueue, ICommittedEventExecutor, EventStream>,
        ICommittedEventProcessor
    {
        public DefaultCommittedEventProcessor(ICommittedEventQueue bindingQueue,  int workerCount = 1)
            : base(bindingQueue, workerCount)
        {
        }
    }
}
