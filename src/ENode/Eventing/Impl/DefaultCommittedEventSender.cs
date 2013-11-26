using ENode.Messaging.Impl;

namespace ENode.Eventing.Impl
{
    /// <summary>The default implementation of ICommittedEventSender.
    /// </summary>
    public class DefaultCommittedEventSender : MessageSender<ICommittedEventQueueRouter, ICommittedEventQueue, EventStream>, ICommittedEventSender
    {
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="eventQueueRouter"></param>
        public DefaultCommittedEventSender(ICommittedEventQueueRouter eventQueueRouter) : base(eventQueueRouter) { }
    }
}
