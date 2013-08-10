using System;
using ENode.Eventing;
using ENode.Infrastructure;
using NoteSample.Events;

namespace NoteSample.EventSubscribers {
    [Component]
    public class NoteEventHandler :
        IEventHandler<NoteCreated>,
        IEventHandler<NoteTitleChanged> {
        public void Handle(NoteCreated evnt) {
            Console.WriteLine(string.Format("Note created, title：{0}", evnt.Title));
        }
        public void Handle(NoteTitleChanged evnt) {
            Console.WriteLine(string.Format("Note title changed, title：{0}", evnt.Title));
        }
    }
}
