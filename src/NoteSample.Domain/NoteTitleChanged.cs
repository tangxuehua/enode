using System;
using ENode.Eventing;
using ENode.Infrastructure;

namespace NoteSample.Domain
{
    [Code(101)]
    public class NoteTitleChanged : DomainEvent<string>
    {
        public string Title { get; private set; }

        private NoteTitleChanged() { }
        public NoteTitleChanged(Note note, string title) : base(note)
        {
            Title = title;
        }
    }
    [Code(102)]
    public class Event1 : DomainEvent<string>
    {
        private Event1() { }
        public Event1(Note note) : base(note)
        {
        }
    }
    [Code(103)]
    public class Event2 : DomainEvent<string>
    {
        private Event2() { }
        public Event2(Note note) : base(note)
        {
        }
    }
    [Code(104)]
    public class Event3 : DomainEvent<string>
    {
        private Event3() { }
        public Event3(Note note) : base(note)
        {
        }
    }
}
