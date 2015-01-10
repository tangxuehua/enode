using DistributeSample.Events;
using ECommon.Components;
using ENode.Eventing;
using ENode.Infrastructure.Impl;

namespace DistributeSample.CommandProcessor.Providers
{
    [Component]
    public class EventTypeCodeProvider : DefaultTypeCodeProvider<IEvent>
    {
        public EventTypeCodeProvider()
        {
            RegisterType<NoteCreated>(100);
        }
    }
}
