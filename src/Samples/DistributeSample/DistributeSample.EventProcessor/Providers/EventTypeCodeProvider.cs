using DistributeSample.Events;
using ENode.Eventing;
using ENode.Infrastructure;

namespace DistributeSample.EventProcessor.Providers
{
    public class EventTypeCodeProvider : AbstractTypeCodeProvider<IEvent>, ITypeCodeProvider<IEvent>
    {
        public EventTypeCodeProvider()
        {
            RegisterType<NoteCreatedEvent>(100);
        }
    }
}
