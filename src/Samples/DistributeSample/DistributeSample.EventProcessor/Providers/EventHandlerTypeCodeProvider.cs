using DistributeSample.EventProcessor.EventHandlers;
using ENode.Eventing;
using ENode.Infrastructure;

namespace DistributeSample.EventProcessor.Providers
{
    public class EventHandlerTypeCodeProvider : AbstractTypeCodeProvider<IEventHandler>, ITypeCodeProvider<IEventHandler>
    {
        public EventHandlerTypeCodeProvider()
        {
            RegisterType<NoteEventHandler>(100);
        }
    }
}
