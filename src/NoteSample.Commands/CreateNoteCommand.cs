using System;
using ENode.Commanding;

namespace NoteSample.Commands
{
    [Serializable]
    public class CreateNoteCommand : Command<string>
    {
        public string Title { get; set; }
    }
}
