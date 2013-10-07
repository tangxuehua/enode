using System;
using ENode.Eventing;

namespace NoteSample.Events
{
    [Serializable]
    public class NoteTitleChanged : Event
    {
        public Guid NoteId { get; private set; }
        public string Title { get; private set; }
        public DateTime UpdatedTime { get; private set; }

        public NoteTitleChanged(Guid noteId, string title, DateTime updatedTime) : base(noteId)
        {
            NoteId = noteId;
            Title = title;
            UpdatedTime = updatedTime;
        }
    }
}
