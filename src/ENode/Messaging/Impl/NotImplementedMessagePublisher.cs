using System;
using ENode.Infrastructure;

namespace ENode.Messaging.Impl
{
    public class NotImplementedMessagePublisher : IPublisher<IMessage>
    {
        public void Publish(IMessage message)
        {
            throw new NotImplementedException();
        }
    }
}
