using ENode.Messaging.Impl;

namespace ENode.Eventing.Impl
{
    /// <summary>The default implementation of ICommittedEventQueue.
    /// </summary>
    public class DefaultCommittedEventQueue : MessageQueue<EventStream>, ICommittedEventQueue
    {
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="queueName"></param>
        public DefaultCommittedEventQueue(string queueName) : base(queueName) { }
    }
}
