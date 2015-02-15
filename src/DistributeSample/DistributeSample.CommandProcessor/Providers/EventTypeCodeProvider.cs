using ECommon.Components;
using ENode.Eventing;
using ENode.Infrastructure.Impl;
using NoteSample.Domain;

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
