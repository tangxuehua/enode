using System;
using ENode.Eventing;

namespace NoteSample.DomainEvents
{
    [Serializable]
    public class NoteTitleChanged : DomainEvent<Guid>, ISourcingEvent
    {
        public string Title { get; private set; }
        public DateTime UpdatedTime { get; private set; }

        public NoteTitleChanged(Guid noteId, string title, DateTime updatedTime) : base(noteId)
        {
            Title = title;
            UpdatedTime = updatedTime;
        }
    }
}
