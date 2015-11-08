using System;
using ENode.Commanding;
using ENode.Infrastructure;

namespace NoteSample.Commands
{
    [Code(1001)]
    public class ChangeNoteTitleCommand : Command<string>
    {
        public string Title { get; set; }
    }
}
