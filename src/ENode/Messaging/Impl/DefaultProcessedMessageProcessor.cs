namespace ENode.Messaging.Impl
{
    /// <summary>Represents a processor to process the processed messages.
    /// </summary>
    public class DefaultProcessedMessageProcessor : MessageProcessor<IProcessedMessageQueue, IProcessedMessageExecutor, IMessage>, IProcessedMessageProcessor
    {
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="bindingQueue"></param>
        /// <param name="workerCount"></param>
        /// <param name="commandDequeueIntervalMilliseconds"></param>
        public DefaultProcessedMessageProcessor(IProcessedMessageQueue bindingQueue, int workerCount = 1, int commandDequeueIntervalMilliseconds = 0)
            : base(bindingQueue, workerCount, commandDequeueIntervalMilliseconds)
        {
        }
    }
}
