using System;
using ENode.Commanding;
using ENode.Infrastructure;

namespace NoteSample.Commands
{
    [Code(101)]
    public class ChangeNoteTitleCommand : Command<string>
    {
        public string Title { get; set; }
    }
}
