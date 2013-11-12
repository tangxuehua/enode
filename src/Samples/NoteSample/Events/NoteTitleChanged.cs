using System;
using ENode.Eventing;

namespace NoteSample.Events
{
    [Serializable]
    public class NoteTitleChanged : SourcableDomainEvent<Guid>
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
