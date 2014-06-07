using System;
using ECommon.Utilities;
using ENode.Commanding;

namespace NoteSample.Commands
{
    [Serializable]
    public class ChangeNoteTitleCommand : Command<string>
    {
        public string Title { get; private set; }

        public ChangeNoteTitleCommand(string noteId, string title) : base(noteId)
        {
            Title = title;
        }
    }
}
