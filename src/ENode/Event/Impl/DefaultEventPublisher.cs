using System;

namespace ENode.Eventing
{
    public class DefaultEventPublisher : IEventPublisher
    {
        private ICommittedEventQueueRouter _eventQueueRouter;

        public DefaultEventPublisher(ICommittedEventQueueRouter eventQueueRouter)
        {
            _eventQueueRouter = eventQueueRouter;
        }

        public void Publish(EventStream stream)
        {
            var eventQueue = _eventQueueRouter.Route(stream);
            if (eventQueue == null)
            {
                throw new Exception("Could not route event stream to an appropriate committed event queue.");
            }

            eventQueue.Enqueue(stream);
        }
    }
}
