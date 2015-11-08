using System;
using ENode.Eventing;
using ENode.Infrastructure;

namespace NoteSample.Domain
{
    [Code(2000)]
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
