using ENode.Messaging.Impl;

namespace ENode.Eventing.Impl
{
    /// <summary>The default implementation of IUncommittedEventQueue.
    /// </summary>
    public class DefaultUncommittedEventQueue : MessageQueue<EventStream>, IUncommittedEventQueue
    {
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="queueName"></param>
        public DefaultUncommittedEventQueue(string queueName) : base(queueName) { }
    }
}
