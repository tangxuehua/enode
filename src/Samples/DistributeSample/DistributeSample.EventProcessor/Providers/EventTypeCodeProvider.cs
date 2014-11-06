using DistributeSample.Events;
using ECommon.Components;
using ENode.Eventing;
using ENode.Infrastructure.Impl;

namespace DistributeSample.EventProcessor.Providers
{
    [Component]
    public class EventTypeCodeProvider : DefaultTypeCodeProvider<IEvent>
    {
        public EventTypeCodeProvider()
        {
            RegisterType<NoteCreatedEvent>(100);
        }
    }
}
