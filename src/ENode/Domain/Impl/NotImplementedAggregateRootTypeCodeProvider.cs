using ENode.Infrastructure;
using ENode.Infrastructure.Impl;

namespace ENode.Domain.Impl
{
    public class NotImplementedAggregateRootTypeCodeProvider : DefaultTypeCodeProvider<IAggregateRoot>, ITypeCodeProvider<IAggregateRoot>
    {
    }
}
