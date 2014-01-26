using System;
using ENode.Commanding;

namespace NoteSample.Commands
{
    [Serializable]
    public class CreateNoteCommand : Command, ICreatingAggregateCommand
    {
        public Guid NoteId { get; private set; }
        public string Title { get; private set; }

        public CreateNoteCommand(Guid noteId, string title) : base(noteId)
        {
            NoteId = noteId;
            Title = title;
        }
    }
}
