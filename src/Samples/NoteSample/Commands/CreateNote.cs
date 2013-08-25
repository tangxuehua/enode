using System;
using ENode.Commanding;

namespace NoteSample.Commands
{
    [Serializable]
    public class CreateNote : Command
    {
        public Guid NoteId { get; set; }
        public string Title { get; set; }
    }
}
