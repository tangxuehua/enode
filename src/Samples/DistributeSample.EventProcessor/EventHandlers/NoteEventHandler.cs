using System;
using DistributeSample.Events;
using ECommon.Components;
using ENode.Eventing;

namespace DistributeSample.EventProcessor.EventHandlers
{
    [Component]
    public class NoteEventHandler : IEventHandler<NoteCreatedEvent>
    {
        public void Handle(IEventContext context, NoteCreatedEvent evnt)
        {
            Console.WriteLine("Note created, Title：{0}", evnt.Title);
        }
    }
}
