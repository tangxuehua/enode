using ENode.Messaging.Impl;

namespace ENode.Eventing.Impl
{
    public class DefaultUncommittedEventQueue : MessageQueue<EventStream>, IUncommittedEventQueue
    {
        public DefaultUncommittedEventQueue(string queueName) : base(queueName) { }
    }
}
