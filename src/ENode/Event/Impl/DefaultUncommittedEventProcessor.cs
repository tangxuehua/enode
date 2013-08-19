using ENode.Messaging;
using ENode.Messaging.Impl;

namespace ENode.Eventing {
    public class DefaultUncommittedEventProcessor :
        MessageProcessor<IUncommittedEventQueue, IUncommittedEventExecutor, EventStream>,
        IUncommittedEventProcessor {
        public DefaultUncommittedEventProcessor(IUncommittedEventQueue bindingQueue, int workerCount = 1)
            : base(bindingQueue, workerCount) {
        }
    }
}
