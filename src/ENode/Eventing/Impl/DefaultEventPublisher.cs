using System;
using ENode.Messaging;

namespace ENode.Eventing.Impl
{
    /// <summary>The default implementation of IEventPublisher.
    /// </summary>
    public class DefaultEventPublisher : IEventPublisher
    {
        private readonly ICommittedEventQueueRouter _eventQueueRouter;

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="eventQueueRouter"></param>
        public DefaultEventPublisher(ICommittedEventQueueRouter eventQueueRouter)
        {
            _eventQueueRouter = eventQueueRouter;
        }

        /// <summary>Publish a given committed event stream to all the event handlers.
        /// </summary>
        /// <param name="stream"></param>
        public void Publish(EventStream eventStream)
        {
            var eventQueue = _eventQueueRouter.Route(eventStream);
            if (eventQueue == null)
            {
                throw new Exception("Could not route event stream to an appropriate committed event queue.");
            }

            eventQueue.Enqueue(new Message<EventStream>(Guid.NewGuid(), eventStream, eventQueue.Name));
        }
    }
}
