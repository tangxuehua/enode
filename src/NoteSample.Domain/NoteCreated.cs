using ENode.Eventing;

namespace NoteSample.Domain
{
    public class NoteCreated : DomainEvent<string>
    {
        public string Title { get; private set; }

        public NoteCreated() { }
        public NoteCreated(string title)
        {
            Title = title;
        }
    }
}
