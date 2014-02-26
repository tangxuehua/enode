using System;
using ENode.Eventing;
using NoteSample.Domain;

namespace NoteSample.DomainEvents
{
    [Serializable]
    public class NoteCreatedEvent : DomainEvent<Guid>
    {
        public string Title { get; private set; }

        public NoteCreatedEvent(Guid id, string title) : base(id)
        {
            Title = title;
        }
    }
}
