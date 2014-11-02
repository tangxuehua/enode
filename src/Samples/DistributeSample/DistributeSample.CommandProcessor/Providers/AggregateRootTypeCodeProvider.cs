using DistributeSample.CommandProcessor.Domain;
using ECommon.Components;
using ENode.Domain;
using ENode.Infrastructure;

namespace DistributeSample.CommandProcessor.Providers
{
    [Component]
    public class AggregateRootTypeCodeProvider : AbstractTypeCodeProvider<IAggregateRoot>
    {
        public AggregateRootTypeCodeProvider()
        {
            RegisterType<Note>(100);
        }
    }
}
