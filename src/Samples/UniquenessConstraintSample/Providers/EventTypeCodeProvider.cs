using ENode.Eventing;
using ENode.Infrastructure;

namespace UniquenessConstraintSample.Providers
{
    public class EventTypeCodeProvider : AbstractTypeCodeProvider<IEvent>, ITypeCodeProvider<IEvent>
    {
        public EventTypeCodeProvider()
        {
            RegisterType<SectionCreatedEvent>(100);
            RegisterType<SectionNameChangedEvent>(101);
        }
    }
}
