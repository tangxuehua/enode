using System;
using ENode.Infrastructure;

namespace ENode.Exceptions.Impl
{
    public class NotImplementedExceptionPublisher : IMessagePublisher<IException>
    {
        public void Publish(IException exception)
        {
            throw new NotImplementedException();
        }
    }
}
