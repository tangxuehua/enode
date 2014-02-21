using System;
using DistributeSample.Events;
using ENode.Domain;

namespace DistributeSample.CommandProcessor.Domain
{
    [Serializable]
    public class Note : AggregateRoot<Guid>
    {
        public string Title { get; private set; }
        public DateTime CreatedTime { get; private set; }
        public DateTime UpdatedTime { get; private set; }

        public Note(Guid id, string title) : base(id)
        {
            var currentTime = DateTime.Now;
            RaiseEvent(new NoteCreatedEvent(id, title, currentTime, currentTime));
        }

        private void Handle(NoteCreatedEvent evnt)
        {
            Id = evnt.AggregateRootId;
            Title = evnt.Title;
            CreatedTime = evnt.CreatedTime;
            UpdatedTime = evnt.UpdatedTime;
        }
    }
}
