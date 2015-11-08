using System;
using ENode.Commanding;
using ENode.Infrastructure;

namespace NoteSample.Commands
{
    [Code(1000)]
    public class CreateNoteCommand : Command<string>
    {
        public string Title { get; set; }
    }
}
