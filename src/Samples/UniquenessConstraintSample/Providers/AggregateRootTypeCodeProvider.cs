using ENode.Domain;
using ENode.Infrastructure;

namespace UniquenessConstraintSample.Providers
{
    public class AggregateRootTypeCodeProvider : AbstractTypeCodeProvider<IAggregateRoot>, ITypeCodeProvider<IAggregateRoot>
    {
        public AggregateRootTypeCodeProvider()
        {
            RegisterType<Section>(100);
        }
    }
}
