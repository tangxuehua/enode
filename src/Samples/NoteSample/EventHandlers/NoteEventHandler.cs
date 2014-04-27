using System;
using ECommon.Components;
using ENode.Eventing;
using NoteSample.DomainEvents;

namespace NoteSample.EventHandlers
{
    [Component]
    public class NoteEventHandler : IEventHandler<NoteCreatedEvent>, IEventHandler<NoteTitleChangedEvent>
    {
        public void Handle(NoteCreatedEvent evnt)
        {
            Console.WriteLine("Note Created, Title：{0}, Version: {1}", evnt.Title, evnt.Version);
        }
        public void Handle(NoteTitleChangedEvent evnt)
        {
            Console.WriteLine("Note Updated, Title：{0}, Version: {1}", evnt.Title, evnt.Version);
        }
    }
}
