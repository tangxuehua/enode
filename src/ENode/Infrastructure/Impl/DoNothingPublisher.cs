using System;
using ENode.Eventing;
using ENode.Exceptions;
using ENode.Infrastructure;

namespace ENode.Messaging.Impl
{
    public class DoNothingPublisher :
        IPublisher<EventStream>,
        IPublisher<DomainEventStream>,
        IPublisher<IEvent>,
        IPublisher<IMessage>,
        IPublisher<IPublishableException>
    {
        public void Publish(IEvent evnt)
        {
        }
        public void Publish(DomainEventStream eventStream)
        {
        }
        public void Publish(EventStream eventStream)
        {
        }
        public void Publish(IMessage message)
        {
        }
        public void Publish(IPublishableException exception)
        {
        }
    }
}
