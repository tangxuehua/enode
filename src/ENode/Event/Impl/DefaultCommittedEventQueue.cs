using ENode.Messaging;

namespace ENode.Eventing {
    public class DefaultCommittedEventQueue : MessageQueue<EventStream>, ICommittedEventQueue {
        public DefaultCommittedEventQueue(string queueName) : base(queueName) { }
    }
}
