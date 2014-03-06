using DistributeSample.Events;
using ENode.Eventing;
using ENode.Infrastructure;

namespace DistributeSample.CommandProcessor.Providers
{
    public class EventTypeCodeProvider : AbstractTypeCodeProvider, IEventTypeCodeProvider
    {
        public EventTypeCodeProvider()
        {
            RegisterType<NoteCreatedEvent>(100);
        }
    }
}
