using System;

namespace ENode.Eventing
{
    public interface IEventTypeCodeProvider
    {
        int GetTypeCode(IDomainEvent domainEvent);
        Type GetType(int typeCode);
    }
}
