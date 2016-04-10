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
}
