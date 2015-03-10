using System;
using ENode.Commanding;

namespace NoteSample.Commands
{
    [Serializable]
    public class ChangeNoteTitleCommand : Command<string>
    {
        public string Title { get; set; }
    }
}
