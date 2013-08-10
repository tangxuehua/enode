using ENode.Messaging;

namespace ENode.Eventing {
    public class DefaultUncommittedEventQueue : MessageQueue<EventStream>, IUncommittedEventQueue {
        public DefaultUncommittedEventQueue(string queueName) : base(queueName) { }
    }
}
