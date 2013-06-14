using System;

namespace ENode.Eventing
{
    public class DefaultEventPublisher : IEventPublisher
    {
        private IEventQueueRouter _eventQueueRouter;

        public DefaultEventPublisher(IEventQueueRouter eventQueueRouter)
        {
            _eventQueueRouter = eventQueueRouter;
        }

        public void Publish(EventStream stream)
        {
            var eventQueue = _eventQueueRouter.Route(stream);
            if (eventQueue == null)
            {
                throw new Exception("Could not route event stream to an appropriate event queue.");
            }

            eventQueue.Enqueue(stream);
        }
    }
}
