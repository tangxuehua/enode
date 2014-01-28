using System;
using DistributeSample.Events;
using ECommon.IoC;
using ENode.Eventing;

namespace DistributeSample.EventProcessor.EventHandlers
{
    [Component]
    public class NoteEventHandler : IEventHandler<NoteCreatedEvent>, IEventHandler<NoteTitleChangedEvent>
    {
        public void Handle(NoteCreatedEvent evnt)
        {
            Console.WriteLine("Note created, Title：{0}", evnt.Title);
        }
        public void Handle(NoteTitleChangedEvent evnt)
        {
            Console.WriteLine("Note updated, Title：{0}", evnt.Title);
        }
    }
}
