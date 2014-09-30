using System;
using System.Collections.Generic;

namespace ENode.Eventing.Impl
{
    public class NotImplementedEventPublisher : IEventPublisher, IDomainEventPublisher
    {
        public void Publish(EventStream eventStream)
        {
            throw new NotImplementedException();
        }
        public void Publish(DomainEventStream eventStream)
        {
            throw new NotImplementedException();
        }
    }
}
