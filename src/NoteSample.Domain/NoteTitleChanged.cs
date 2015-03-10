using System;
using ENode.Eventing;

namespace NoteSample.Domain
{
    [Serializable]
    public class NoteTitleChanged : DomainEvent<string>
    {
        public string Title { get; private set; }

        private NoteTitleChanged() { }
        public NoteTitleChanged(Note note, string title) : base(note)
        {
            Title = title;
        }
    }
}
