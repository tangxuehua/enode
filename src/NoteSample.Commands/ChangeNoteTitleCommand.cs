using System;
using ENode.Commanding;

namespace NoteSample.Commands
{
    public class ChangeNoteTitleCommand : Command<string>
    {
        public string Title { get; set; }
    }
}
