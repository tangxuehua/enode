using System;
using ENode.Eventing;

namespace DistributeSample.Events
{
    [Serializable]
    public class NoteCreatedEvent : SourcingEvent<Guid>
    {
        public string Title { get; private set; }
        public DateTime CreatedTime { get; private set; }
        public DateTime UpdatedTime { get; private set; }

        public NoteCreatedEvent(Guid noteId, string title, DateTime createdTime, DateTime updatedTime) : base(noteId)
        {
            Title = title;
            CreatedTime = createdTime;
            UpdatedTime = updatedTime;
        }
    }
}
