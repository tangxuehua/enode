using ENode.Domain;
using ENode.Infrastructure;

namespace UniquenessConstraintSample.Providers
{
    public class AggregateRootTypeCodeProvider : AbstractTypeCodeProvider, IAggregateRootTypeCodeProvider
    {
        public AggregateRootTypeCodeProvider()
        {
            RegisterType<Section>(100);
        }
    }
}
