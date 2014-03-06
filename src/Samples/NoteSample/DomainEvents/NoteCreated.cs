using System;
using ENode.Eventing;
using NoteSample.Domain;

namespace NoteSample.DomainEvents
{
    [Serializable]
    public class NoteCreatedEvent : DomainEvent<string>
    {
        public string Title { get; private set; }

        public NoteCreatedEvent(string id, string title) : base(id)
        {
            Title = title;
        }
    }
}
