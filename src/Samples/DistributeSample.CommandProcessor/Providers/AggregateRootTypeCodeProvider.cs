using DistributeSample.CommandProcessor.Domain;
using ENode.Domain;
using ENode.Infrastructure;

namespace DistributeSample.CommandProcessor.Providers
{
    public class AggregateRootTypeCodeProvider : AbstractTypeCodeProvider, IAggregateRootTypeCodeProvider
    {
        public AggregateRootTypeCodeProvider()
        {
            RegisterType<Note>(100);
        }
    }
}
