using System;
using ENode.Commanding;
using ENode.Infrastructure;

namespace NoteSample.Commands
{
    [Code(100)]
    public class CreateNoteCommand : Command<string>
    {
        public string Title { get; set; }
    }
}
