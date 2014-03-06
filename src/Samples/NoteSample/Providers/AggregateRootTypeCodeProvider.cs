using ENode.Domain;
using ENode.Infrastructure;
using NoteSample.Domain;

namespace NoteSample.Providers
{
    public class AggregateRootTypeCodeProvider : AbstractTypeCodeProvider, IAggregateRootTypeCodeProvider
    {
        public AggregateRootTypeCodeProvider()
        {
            RegisterType<Note>(100);
        }
    }
}
