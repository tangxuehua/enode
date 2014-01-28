using System;
using ENode.Eventing;

namespace DistributeSample.Events
{
    [Serializable]
    public class NoteTitleChangedEvent : SourcingEvent<Guid>
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
