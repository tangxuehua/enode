using System;
using ENode.Infrastructure;

namespace ENode.Eventing.Impl
{
    public class NotImplementedEventPublisher : IMessagePublisher<EventStream>, IMessagePublisher<DomainEventStream>, IEventPublisher
    {
        public void Publish(EventStream eventStream)
        {
            throw new NotImplementedException();
        }
        public void Publish(DomainEventStream eventStream)
        {
            throw new NotImplementedException();
        }
        public void Publish(IEvent evnt)
        {
            throw new NotImplementedException();
        }
    }
}
