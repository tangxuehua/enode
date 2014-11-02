using DistributeSample.EventProcessor.EventHandlers;
using ECommon.Components;
using ENode.Eventing;
using ENode.Infrastructure;

namespace DistributeSample.EventProcessor.Providers
{
    [Component]
    public class EventHandlerTypeCodeProvider : AbstractTypeCodeProvider<IEventHandler>, ITypeCodeProvider<IEventHandler>
    {
        public EventHandlerTypeCodeProvider()
        {
            RegisterType<NoteEventHandler>(100);
        }
    }
}
