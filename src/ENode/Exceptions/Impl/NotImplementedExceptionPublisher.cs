using System;
using ENode.Infrastructure;

namespace ENode.Exceptions.Impl
{
    public class NotImplementedExceptionPublisher : IMessagePublisher<IPublishableException>
    {
        public void Publish(IPublishableException exception)
        {
            throw new NotImplementedException();
        }
    }
}
