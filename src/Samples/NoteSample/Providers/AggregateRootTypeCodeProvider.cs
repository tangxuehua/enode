using ECommon.Components;
using ENode.Domain;
using ENode.Infrastructure;
using NoteSample.Domain;

namespace NoteSample.Providers
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
