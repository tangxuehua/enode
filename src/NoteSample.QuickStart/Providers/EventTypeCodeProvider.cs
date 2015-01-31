using ECommon.Components;
using ENode.Eventing;
using ENode.Infrastructure.Impl;
using NoteSample.Domain;

namespace NoteSample.QuickStart.Providers
{
    [Component]
    public class EventTypeCodeProvider : DefaultTypeCodeProvider<IEvent>
    {
        public EventTypeCodeProvider()
        {
            RegisterType<NoteCreated>(100);
            RegisterType<NoteTitleChanged>(101);
        }
    }
}
