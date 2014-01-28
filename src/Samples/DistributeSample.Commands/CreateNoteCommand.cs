using System;
using ENode.Commanding;

namespace DistributeSample.Commands
{
    [Serializable]
    public class CreateNoteCommand : Command<Guid>, ICreatingAggregateCommand
    {
        public string Title { get; private set; }

        public CreateNoteCommand(Guid noteId, string title) : base(noteId)
        {
            Title = title;
        }
    }
}
