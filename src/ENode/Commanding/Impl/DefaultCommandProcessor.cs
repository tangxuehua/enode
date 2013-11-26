using ENode.Messaging.Impl;

namespace ENode.Commanding.Impl
{
    /// <summary>The default command processor.
    /// </summary>
    public class DefaultCommandProcessor : MessageProcessor<ICommandQueue, ICommandMessageHandler, ICommand>, ICommandProcessor
    {
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="bindingQueue"></param>
        /// <param name="workerCount"></param>
        /// <param name="commandDequeueIntervalMilliseconds"></param>
        public DefaultCommandProcessor(ICommandQueue bindingQueue, int workerCount = 1, int commandDequeueIntervalMilliseconds = 0)
            : base(bindingQueue, workerCount, commandDequeueIntervalMilliseconds)
        {
        }
    }
}
