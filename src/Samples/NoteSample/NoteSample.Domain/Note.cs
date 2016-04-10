using ENode.Domain;

namespace NoteSample.Domain
{
    public class Note : AggregateRoot<string>
    {
        private string _title;

        public string Title { get { return _title; } }

        public Note(string id, string title) : base(id)
        {
            ApplyEvent(new NoteCreated(title));
        }

        public void ChangeTitle(string title)
        {
            ApplyEvent(new NoteTitleChanged(title));
        }

        private void Handle(NoteCreated evnt)
        {
            _title = evnt.Title;
        }
        private void Handle(NoteTitleChanged evnt)
        {
            _title = evnt.Title;
        }
    }
}
