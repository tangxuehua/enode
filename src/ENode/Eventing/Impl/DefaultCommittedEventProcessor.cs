using ENode.Messaging.Impl;

namespace ENode.Eventing.Impl
{
    /// <summary>
    /// 
    /// </summary>
    public class DefaultCommittedEventProcessor :
        MessageProcessor<ICommittedEventQueue, ICommittedEventExecutor, EventStream>,
        ICommittedEventProcessor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bindingQueue"></param>
        /// <param name="workerCount"></param>
        public DefaultCommittedEventProcessor(ICommittedEventQueue bindingQueue, int workerCount = 1)
            : base(bindingQueue, workerCount)
        {
        }
    }
}
