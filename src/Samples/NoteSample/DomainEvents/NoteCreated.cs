using System;
using ENode.Eventing;

namespace NoteSample.DomainEvents
{
    [Serializable]
    public class NoteCreated : DomainEvent<string>
    {
        public string Title { get; private set; }

        public NoteCreated(string id, string title) : base(id)
        {
            Title = title;
        }
    }
}
