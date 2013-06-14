using ENode.Messaging;

namespace ENode.Eventing
{
    public class DefaultEventProcessor :
        Processor<IEventQueue, IEventExecutor, EventStream>,
        IEventProcessor
    {
        public DefaultEventProcessor(IEventQueue bindingQueue,  int workerCount = 1)
            : base(bindingQueue, workerCount)
        {
        }
    }
}
