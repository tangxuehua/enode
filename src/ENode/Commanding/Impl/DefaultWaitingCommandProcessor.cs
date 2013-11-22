using ENode.Messaging.Impl;

namespace ENode.Commanding.Impl
{
    /// <summary>The waiting command processor.
    /// </summary>
    public class DefaultWaitingCommandProcessor : MessageProcessor<ICommandQueue, IWaitingCommandExecutor, ICommand>, ICommandProcessor
    {
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="bindingQueue"></param>
        /// <param name="workerCount"></param>
        /// <param name="commandDequeueIntervalMilliseconds"></param>
        public DefaultWaitingCommandProcessor(ICommandQueue bindingQueue, int workerCount = 1, int commandDequeueIntervalMilliseconds = 0)
            : base(bindingQueue, workerCount, commandDequeueIntervalMilliseconds)
        {
        }
    }
}
