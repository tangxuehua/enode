using System;

namespace ENode.Eventing.Impl
{
    public class NotImplementedEventTypeCodeProvider : IEventTypeCodeProvider
    {
        public int GetTypeCode(IDomainEvent domainEvent)
        {
            throw new NotImplementedException();
        }
        public Type GetType(int typeCode)
        {
            throw new NotImplementedException();
        }
    }
}
