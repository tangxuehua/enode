using System;
using ENode.Eventing;

namespace NoteSample.Domain
{
    public class NoteTitleChanged : DomainEvent<string>
    {
        public string Title { get; private set; }

        private NoteTitleChanged() { }
        public NoteTitleChanged(Note note, string title) : base(note)
        {
            Title = title;
        }
    }
    public class Event1 : DomainEvent<string>
    {
        private Event1() { }
        public Event1(Note note) : base(note)
        {
        }
    }
    public class Event2 : DomainEvent<string>
    {
        private Event2() { }
        public Event2(Note note) : base(note)
        {
        }
    }
    public class Event3 : DomainEvent<string>
    {
        private Event3() { }
        public Event3(Note note) : base(note)
        {
        }
    }
}
