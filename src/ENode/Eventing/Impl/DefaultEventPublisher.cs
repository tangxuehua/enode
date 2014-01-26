using ENode.Messaging.Impl;

namespace ENode.Eventing.Impl
{
    /// <summary>The default implementation of IEventPublisher.
    /// </summary>
    public class DefaultEventPublisher : MessageSender<ICommittedEventQueueRouter, ICommittedEventQueue, EventStream>, IEventPublisher
    {
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="eventQueueRouter"></param>
        public DefaultEventPublisher(ICommittedEventQueueRouter eventQueueRouter) : base(eventQueueRouter) { }
    }
}
