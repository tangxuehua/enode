using DistributeSample.Events;
using ECommon.Components;
using ENode.Eventing;
using ENode.Infrastructure;

namespace DistributeSample.EventProcessor.Providers
{
    [Component(LifeStyle.Singleton)]
    public class EventTypeCodeProvider : AbstractTypeCodeProvider<IEvent>, ITypeCodeProvider<IEvent>
    {
        public EventTypeCodeProvider()
        {
            RegisterType<NoteCreatedEvent>(100);
        }
    }
}
