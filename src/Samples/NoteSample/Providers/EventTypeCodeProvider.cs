using ENode.Eventing;
using ENode.Infrastructure;
using NoteSample.DomainEvents;

namespace NoteSample.Providers
{
    public class EventTypeCodeProvider : AbstractTypeCodeProvider, IEventTypeCodeProvider
    {
        public EventTypeCodeProvider()
        {
            RegisterType<NoteCreatedEvent>(100);
            RegisterType<NoteTitleChangedEvent>(101);
        }
    }
}
