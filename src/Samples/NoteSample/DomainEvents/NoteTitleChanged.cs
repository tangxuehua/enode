using System;
using ENode.Eventing;

namespace NoteSample.DomainEvents
{
    [Serializable]
    public class NoteTitleChangedEvent : DomainEvent<Guid>
    {
        public string Title { get; private set; }
        public DateTime UpdatedTime { get; private set; }

        public NoteTitleChangedEvent(Guid noteId, string title, DateTime updatedTime) : base(noteId)
        {
            Title = title;
            UpdatedTime = updatedTime;
        }
    }
}
