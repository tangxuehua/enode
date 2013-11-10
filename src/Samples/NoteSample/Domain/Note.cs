using System;
using ENode.Domain;
using ENode.Eventing;
using NoteSample.Events;

namespace NoteSample.Domain
{
    [Serializable]
    public class Note : AggregateRoot<Guid>
    {
        public string Title { get; private set; }
        public DateTime CreatedTime { get; private set; }
        public DateTime UpdatedTime { get; private set; }

        public Note(Guid id, string title)
        {
            var currentTime = DateTime.Now;
            RaiseEvent(new NoteCreated(id, title, currentTime, currentTime));
        }

        public void ChangeTitle(string title)
        {
            RaiseEvent(new NoteTitleChanged(Id, title, DateTime.Now));
        }

        private void Handle(NoteCreated evnt)
        {
            Id = evnt.SourceId;
            Title = evnt.Title;
            CreatedTime = evnt.CreatedTime;
            UpdatedTime = evnt.UpdatedTime;
        }
        private void Handle(NoteTitleChanged evnt)
        {
            Title = evnt.Title;
            UpdatedTime = evnt.UpdatedTime;
        }
    }
}
