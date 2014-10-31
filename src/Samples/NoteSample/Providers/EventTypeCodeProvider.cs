using ECommon.Components;
using ENode.Eventing;
using ENode.Infrastructure;
using NoteSample.DomainEvents;

namespace NoteSample.Providers
{
    [Component(LifeStyle.Singleton)]
    public class EventTypeCodeProvider : AbstractTypeCodeProvider<IEvent>
    {
        public EventTypeCodeProvider()
        {
            RegisterType<NoteCreatedEvent>(100);
            RegisterType<NoteTitleChangedEvent>(101);
        }
    }
}
