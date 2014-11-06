using DistributeSample.CommandProcessor.Domain;
using ECommon.Components;
using ENode.Domain;
using ENode.Infrastructure.Impl;

namespace DistributeSample.CommandProcessor.Providers
{
    [Component]
    public class AggregateRootTypeCodeProvider : DefaultTypeCodeProvider<IAggregateRoot>
    {
        public AggregateRootTypeCodeProvider()
        {
            RegisterType<Note>(100);
        }
    }
}
