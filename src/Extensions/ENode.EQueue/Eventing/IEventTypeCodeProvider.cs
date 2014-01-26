using System;
using ENode.Eventing;

namespace ENode.EQueue
{
    public interface IEventTypeCodeProvider
    {
        int GetTypeCode(IDomainEvent domainEvent);
        Type GetType(int typeCode);
    }
}
