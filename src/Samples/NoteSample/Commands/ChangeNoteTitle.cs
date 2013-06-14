using System;
using ENode.Commanding;

namespace NoteSample.Commands
{
    [Serializable]
    public class ChangeNoteTitle : Command
    {
        public Guid NoteId { get; set; }
        public string Title { get; set; }
    }
}
