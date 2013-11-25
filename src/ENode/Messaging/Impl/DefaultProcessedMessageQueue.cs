namespace ENode.Messaging.Impl
{
    /// <summary>The default implementation of ICommandQueue.
    /// </summary>
    public class DefaultProcessedMessageQueue : MessageQueue<IMessage>, IProcessedMessageQueue
    {
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="queueName">The name of the queue.</param>
        public DefaultProcessedMessageQueue(string queueName) : base(queueName) { }
    }
}
