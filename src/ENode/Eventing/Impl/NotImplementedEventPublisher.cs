using System;

namespace ENode.Eventing.Impl
{
    public class NotImplementedEventPublisher : IEventPublisher
    {
        public void PublishEvent(EventStream eventStream)
        {
            throw new NotImplementedException("NotImplementedEventPublisher does not support publishing event.");
        }
    }
}
