using ENode.Eventing;
using ENode.Infrastructure;

namespace UniquenessConstraintSample.Providers
{
    public class EventTypeCodeProvider : AbstractTypeCodeProvider, IEventTypeCodeProvider
    {
        public EventTypeCodeProvider()
        {
            RegisterType<SectionCreatedEvent>(100);
            RegisterType<SectionNameChangedEvent>(101);
        }
    }
}
