using System;
using System.Collections.Generic;

namespace ENode.Eventing.Impl
{
    public class NotImplementedEventPublisher : IEventPublisher
    {
        public void PublishEvent(IDictionary<string, string> contextItems, EventStream eventStream)
        {
            throw new NotImplementedException("NotImplementedEventPublisher does not support publishing event.");
        }
    }
}
