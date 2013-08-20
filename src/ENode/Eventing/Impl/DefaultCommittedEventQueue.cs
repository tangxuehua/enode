using ENode.Messaging.Impl;

namespace ENode.Eventing.Impl
{
    /// <summary>
    /// 
    /// </summary>
    public class DefaultCommittedEventQueue : MessageQueue<EventStream>, ICommittedEventQueue
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="queueName"></param>
        public DefaultCommittedEventQueue(string queueName) : base(queueName) { }
    }
}
