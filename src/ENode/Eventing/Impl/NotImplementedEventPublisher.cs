using System;
using System.Collections.Generic;
using ENode.Infrastructure;

namespace ENode.Eventing.Impl
{
    public class NotImplementedEventPublisher : IMessagePublisher<EventStream>, IMessagePublisher<DomainEventStream>
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
