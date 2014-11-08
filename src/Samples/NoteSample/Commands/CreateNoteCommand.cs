using System;
using ENode.Commanding;

namespace NoteSample.Commands
{
    [Serializable]
    public class CreateNoteCommand : AggregateCommand<string>, ICreatingAggregateCommand
    {
        public string Title { get; set; }
    }
}
