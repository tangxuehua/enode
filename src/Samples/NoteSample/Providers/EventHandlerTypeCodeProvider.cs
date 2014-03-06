using ENode.Eventing;
using ENode.Infrastructure;
using NoteSample.EventHandlers;

namespace NoteSample.Providers
{
    public class EventHandlerTypeCodeProvider : AbstractTypeCodeProvider, IEventHandlerTypeCodeProvider
    {
        public EventHandlerTypeCodeProvider()
        {
            RegisterType<NoteEventHandler>(100);
        }
    }
}
