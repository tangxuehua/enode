using ENode.Messaging.Impl;

namespace ENode.Eventing.Impl
{
    /// <summary>The default implementation of IUncommittedEventSender.
    /// </summary>
    public class DefaultUncommittedEventSender : MessageSender<IUncommittedEventQueueRouter, IUncommittedEventQueue, EventStream>, IUncommittedEventSender
    {
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="eventQueueRouter"></param>
        public DefaultUncommittedEventSender(IUncommittedEventQueueRouter eventQueueRouter) : base(eventQueueRouter) { }
    }
}
