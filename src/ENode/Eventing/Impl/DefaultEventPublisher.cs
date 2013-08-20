using System;

namespace ENode.Eventing.Impl
{
    /// <summary>
    /// 
    /// </summary>
    public class DefaultEventPublisher : IEventPublisher
    {
        private readonly ICommittedEventQueueRouter _eventQueueRouter;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventQueueRouter"></param>
        public DefaultEventPublisher(ICommittedEventQueueRouter eventQueueRouter)
        {
            _eventQueueRouter = eventQueueRouter;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
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
