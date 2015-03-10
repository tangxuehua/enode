using System;
using ENode.Eventing;

namespace NoteSample.Domain
{
    [Serializable]
    public class NoteCreated : DomainEvent<string>
    {
        public string Title { get; private set; }

        private NoteCreated() { }
        public NoteCreated(Note note, string title) : base(note)
        {
            Title = title;
        }
    }
}
