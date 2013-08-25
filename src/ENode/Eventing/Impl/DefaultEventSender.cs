using System;

namespace ENode.Eventing.Impl
{
    /// <summary>The default implementation of IEventSender.
    /// </summary>
    public class DefaultEventSender : IEventSender
    {
        private readonly IUncommittedEventQueueRouter _eventQueueRouter;

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="eventQueueRouter"></param>
        public DefaultEventSender(IUncommittedEventQueueRouter eventQueueRouter)
        {
            _eventQueueRouter = eventQueueRouter;
        }
        /// <summary>Send the uncommitted event stream to process asynchronously.
        /// </summary>
        /// <param name="eventStream"></param>
        public void Send(EventStream eventStream)
        {
            var eventQueue = _eventQueueRouter.Route(eventStream);
            if (eventQueue == null)
            {
                throw new Exception("Could not route event stream to an appropriate uncommitted event queue.");
            }

            eventQueue.Enqueue(eventStream);
        }
    }
}
