using System;
using ENode.Domain;
using ENode.Eventing;

namespace NoteSample.Events {
    [Serializable]
    public class NoteCreated : Event {
        public Guid NoteId { get; private set; }
        public string Title { get; private set; }
        public DateTime CreatedTime { get; private set; }
        public DateTime UpdatedTime { get; private set; }

        public NoteCreated(Guid noteId, string title, DateTime createdTime, DateTime updatedTime) {
            NoteId = noteId;
            Title = title;
            CreatedTime = createdTime;
            UpdatedTime = updatedTime;
        }
    }
}
