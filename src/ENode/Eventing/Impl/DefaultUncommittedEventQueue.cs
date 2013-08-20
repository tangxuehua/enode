using ENode.Messaging.Impl;

namespace ENode.Eventing.Impl
{
    /// <summary>
    /// 
    /// </summary>
    public class DefaultUncommittedEventQueue : MessageQueue<EventStream>, IUncommittedEventQueue
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="queueName"></param>
        public DefaultUncommittedEventQueue(string queueName) : base(queueName) { }
    }
}
