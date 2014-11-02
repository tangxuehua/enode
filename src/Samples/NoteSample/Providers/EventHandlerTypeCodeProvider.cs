using ECommon.Components;
using ENode.Eventing;
using ENode.Infrastructure;
using NoteSample.EventHandlers;

namespace NoteSample.Providers
{
    [Component]
    public class EventHandlerTypeCodeProvider : AbstractTypeCodeProvider<IEventHandler>
    {
        public EventHandlerTypeCodeProvider()
        {
            RegisterType<NoteEventHandler>(100);
        }
    }
}
