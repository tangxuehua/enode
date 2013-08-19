using ENode.Messaging;
using ENode.Messaging.Impl;

namespace ENode.Commanding.Impl
{
    /// <summary>The default implementation of ICommandProcessor.
    /// </summary>
    public class DefaultCommandProcessor : MessageProcessor<ICommandQueue, ICommandExecutor, ICommand>, ICommandProcessor
    {
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="bindingQueue"></param>
        /// <param name="workerCount"></param>
        public DefaultCommandProcessor(ICommandQueue bindingQueue, int workerCount = 1) : base(bindingQueue, workerCount)
        {
        }
    }
}
