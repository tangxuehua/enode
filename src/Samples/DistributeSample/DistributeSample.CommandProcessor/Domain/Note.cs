using System;
using DistributeSample.Events;
using ENode.Domain;

namespace DistributeSample.CommandProcessor.Domain
{
    [Serializable]
    public class Note : AggregateRoot<string>
    {
        private string _title;

        public Note(string id, string title) : base(id)
        {
            ApplyEvent(new NoteCreatedEvent(id, title));
        }

        private void Handle(NoteCreatedEvent evnt)
        {
            _id = evnt.AggregateRootId;
            _title = evnt.Title;
        }
    }
}
