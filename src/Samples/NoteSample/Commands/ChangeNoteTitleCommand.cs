using System;
using ENode.Commanding;

namespace NoteSample.Commands
{
    [Serializable]
    public class ChangeNoteTitleCommand : AggregateCommand<string>
    {
        public string Title { get; set; }
    }
}
