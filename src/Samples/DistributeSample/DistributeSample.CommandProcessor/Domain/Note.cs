using System;
using DistributeSample.Events;
using ENode.Domain;

namespace DistributeSample.CommandProcessor.Domain
{
    [Serializable]
    public class Note : AggregateRoot<string>
    {
        public string Title { get; private set; }

        public Note(string id, string title) : base(id)
        {
            ApplyEvent(new NoteCreatedEvent(Id, title));
        }

        private void Handle(NoteCreatedEvent evnt)
        {
            Id = evnt.AggregateRootId;
            Title = evnt.Title;
        }
    }
}
