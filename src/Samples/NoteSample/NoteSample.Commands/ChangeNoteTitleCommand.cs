using System;
using ENode.Commanding;
using ENode.Infrastructure;

namespace NoteSample.Commands
{
    public class ChangeNoteTitleCommand : Command<string>
    {
        public string Title { get; set; }
    }
}
