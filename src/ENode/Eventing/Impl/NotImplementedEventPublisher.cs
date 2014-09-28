using System;
using System.Collections.Generic;

namespace ENode.Eventing.Impl
{
    public class NotImplementedEventPublisher : IEventPublisher, IDomainEventPublisher
    {
        public void PublishEvent(EventStream eventStream, IDictionary<string, string> contextItems)
        {
            throw new NotImplementedException();
        }
        public void PublishEvent(DomainEventStream eventStream, IDictionary<string, string> contextItems)
        {
            throw new NotImplementedException();
        }
    }
}
