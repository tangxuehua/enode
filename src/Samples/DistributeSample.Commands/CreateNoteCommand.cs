using System;
using ECommon.Utilities;
using ENode.Commanding;

namespace DistributeSample.Commands
{
    [Serializable]
    public class CreateNoteCommand : Command<string>, ICreatingAggregateCommand
    {
        public string Title { get; private set; }

        public CreateNoteCommand(string noteId, string title) : base(noteId)
        {
            Title = title;
        }
    }
}
