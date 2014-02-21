using System;
using ENode.Domain;
using ENode.Eventing;
using NoteSample.DomainEvents;

namespace NoteSample.Domain
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

        public void ChangeTitle(string title)
        {
            RaiseEvent(new NoteTitleChangedEvent(Id, title, DateTime.Now));
        }

        private void Handle(NoteCreatedEvent evnt)
        {
            Id = evnt.AggregateRootId;
            Title = evnt.Title;
            CreatedTime = evnt.CreatedTime;
            UpdatedTime = evnt.UpdatedTime;
        }
        private void Handle(NoteTitleChangedEvent evnt)
        {
            Title = evnt.Title;
            UpdatedTime = evnt.UpdatedTime;
        }
    }
}
