using ENode.Messaging.Impl;

namespace ENode.Eventing.Impl
{
    /// <summary>The default implementation of IUncommittedEventProcessor.
    /// </summary>
    public class DefaultUncommittedEventProcessor :
        MessageProcessor<IUncommittedEventQueue, IUncommittedEventExecutor, EventStream>,
        IUncommittedEventProcessor
    {
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="bindingQueue"></param>
        /// <param name="workerCount"></param>
        public DefaultUncommittedEventProcessor(IUncommittedEventQueue bindingQueue, int workerCount = 1) : base(bindingQueue, workerCount)
        {
        }
    }
}
