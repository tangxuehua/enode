using ENode.Messaging;

namespace ENode.Commanding
{
    /// <summary>The default implementation of ICommandQueue.
    /// </summary>
    public class DefaultCommandQueue : MessageQueue<ICommand>, ICommandQueue
    {
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="queueName">The name of the queue.</param>
        public DefaultCommandQueue(string queueName) : base(queueName) { }
    }
}
