using System;
using ENode.Eventing;
using ENode.Infrastructure;
using NoteSample.DomainEvents;

namespace NoteSample.EventHandlers
{
    [Component]
    public class NoteEventHandler :
        IEventHandler<NoteCreated>,
        IEventHandler<NoteTitleChanged>
    {
        public void Handle(NoteCreated evnt)
        {
            Console.WriteLine("Note created, title：{0}", evnt.Title);
        }
        public void Handle(NoteTitleChanged evnt)
        {
            Console.WriteLine("Note title changed, title：{0}", evnt.Title);
        }
    }
}
