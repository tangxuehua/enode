using ENode.Eventing;

namespace NoteSample.Domain
{
    public class NoteTitleChanged : DomainEvent<string>
    {
        public string Title { get; private set; }

        public NoteTitleChanged() { }
        public NoteTitleChanged(string title)
        {
            Title = title;
        }
    }
    public class Event1 : DomainEvent<string> { }
    public class Event2 : DomainEvent<string> { }
    public class Event3 : DomainEvent<string> { }
}
