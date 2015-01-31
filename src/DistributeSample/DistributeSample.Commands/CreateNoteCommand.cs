using System;
using ENode.Commanding;

namespace DistributeSample.Commands
{
    [Serializable]
    public class CreateNoteCommand : AggregateCommand<string>, ICreatingAggregateCommand
    {
        public string Title { get; set; }
    }
}
