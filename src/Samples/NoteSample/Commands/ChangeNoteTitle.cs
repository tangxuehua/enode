using System;
using ENode.Commanding;

namespace NoteSample.Commands
{
    [Serializable]
    public class ChangeNoteTitle : Command
    {
        public Guid NoteId { get; private set; }
        public string Title { get; private set; }

        public ChangeNoteTitle(Guid noteId, string title) : base(noteId)
        {
            NoteId = noteId;
            Title = title;
        }
    }
}
