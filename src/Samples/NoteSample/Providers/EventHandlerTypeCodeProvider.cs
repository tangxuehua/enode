using ENode.Eventing;
using ENode.Infrastructure;
using NoteSample.EventHandlers;

namespace NoteSample.Providers
{
    public class EventHandlerTypeCodeProvider : AbstractTypeCodeProvider<IEventHandler>, ITypeCodeProvider<IEventHandler>
    {
        public EventHandlerTypeCodeProvider()
        {
            RegisterType<NoteEventHandler>(100);
        }
    }
}
