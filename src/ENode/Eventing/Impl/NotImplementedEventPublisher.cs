using System;
using System.Collections.Generic;

namespace ENode.Eventing.Impl
{
    public class NotImplementedEventPublisher : IEventPublisher
    {
        public void PublishEvent(IDictionary<string, object> contextItems, EventStream eventStream)
        {
            throw new NotImplementedException("NotImplementedEventPublisher does not support publishing event.");
        }
    }
}
