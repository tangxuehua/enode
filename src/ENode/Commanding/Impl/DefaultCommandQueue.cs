using ENode.Eventing;
using ENode.Messaging.Impl;

namespace ENode.Commanding.Impl
{
    /// <summary>The default implementation of ICommandQueue.
    /// </summary>
    public class DefaultCommandQueue : MessageQueue<EventCommittingContext>, ICommandQueue
    {
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="queueName">The name of the queue.</param>
        public DefaultCommandQueue(string queueName) : base(queueName) { }
    }
}
