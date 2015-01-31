using DistributeSample.EventProcessor.EventHandlers;
using ECommon.Components;
using ENode.Eventing;
using ENode.Infrastructure.Impl;

namespace DistributeSample.EventProcessor.Providers
{
    [Component]
    public class EventHandlerTypeCodeProvider : DefaultTypeCodeProvider<IEventHandler>
    {
        public EventHandlerTypeCodeProvider()
        {
            RegisterType<NoteEventHandler>(100);
        }
    }
}
