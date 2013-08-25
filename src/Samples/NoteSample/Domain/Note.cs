using System;
using ENode.Domain;
using ENode.Eventing;
using NoteSample.Events;

namespace NoteSample.Domain
{
    [Serializable]
    public class Note : AggregateRoot<Guid>,
        IEventHandler<NoteCreated>,
        IEventHandler<NoteTitleChanged>
    {
        public string Title { get; private set; }
        public DateTime CreatedTime { get; private set; }
        public DateTime UpdatedTime { get; private set; }

        public Note() { }
        public Note(Guid id, string title)
            : base(id)
        {
            var currentTime = DateTime.Now;
            RaiseEvent(new NoteCreated(Id, title, currentTime, currentTime));
        }

        public void ChangeTitle(string title)
        {
            RaiseEvent(new NoteTitleChanged(Id, title, DateTime.Now));
        }

        void IEventHandler<NoteCreated>.Handle(NoteCreated evnt)
        {
            Title = evnt.Title;
            CreatedTime = evnt.CreatedTime;
            UpdatedTime = evnt.UpdatedTime;
        }
        void IEventHandler<NoteTitleChanged>.Handle(NoteTitleChanged evnt)
        {
            Title = evnt.Title;
            UpdatedTime = evnt.UpdatedTime;
        }
    }
}
