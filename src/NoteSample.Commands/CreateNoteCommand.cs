using System;
using ENode.Commanding;

namespace NoteSample.Commands
{
    public class CreateNoteCommand : Command<string>
    {
        public string Title { get; set; }
    }
}
