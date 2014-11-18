using System;
using ENode.Infrastructure;

namespace ENode.Exceptions.Impl
{
    public class NotImplementedExceptionPublisher : IPublisher<IPublishableException>
    {
        public void Publish(IPublishableException exception)
        {
            throw new NotImplementedException();
        }
    }
}
