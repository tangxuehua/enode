using System;

namespace ENode.Infrastructure
{
    public class NotImplementedExceptionPublisher : IExceptionPublisher
    {
        public void PublishException(IPublishableException exception)
        {
            throw new NotImplementedException();
        }
    }
}
