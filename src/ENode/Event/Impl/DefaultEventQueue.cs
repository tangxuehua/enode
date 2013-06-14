using ENode.Messaging;

namespace ENode.Eventing
{
    public class DefaultEventQueue : QueueBase<EventStream>, IEventQueue
    {
        public DefaultEventQueue(string queueName) : base(queueName) { }
    }
}
