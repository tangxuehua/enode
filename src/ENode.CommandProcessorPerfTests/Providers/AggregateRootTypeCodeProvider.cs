using ECommon.Components;
using ENode.Domain;
using ENode.Infrastructure.Impl;
using NoteSample.Domain;

namespace ENode.CommandProcessorPerfTests.Providers
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
