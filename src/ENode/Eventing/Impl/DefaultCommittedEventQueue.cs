using ENode.Messaging.Impl;

namespace ENode.Eventing.Impl
{
    public class DefaultCommittedEventQueue : MessageQueue<EventStream>, ICommittedEventQueue
    {
        public DefaultCommittedEventQueue(string queueName) : base(queueName) { }
    }
}
