using System;

namespace ENode.Eventing
{
    public class DefaultEventSender : IEventSender
    {
        private IUncommittedEventQueueRouter _eventQueueRouter;

        public DefaultEventSender(IUncommittedEventQueueRouter eventQueueRouter)
        {
            _eventQueueRouter = eventQueueRouter;
        }

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
