using System;
using ENode.Domain;

namespace NoteSample.Domain
{
    [Serializable]
    public class Note : AggregateRoot<string>
    {
        private string _title;

        public string Title { get { return _title; } }

        public Note(string id, string title) : base(id)
        {
            ApplyEvent(new NoteCreated(id, title));
        }

        public void ChangeTitle(string title)
        {
            ApplyEvent(new NoteTitleChanged(Id, title));
        }

        private void Handle(NoteCreated evnt)
        {
            _id = evnt.AggregateRootId;
            _title = evnt.Title;
        }
        private void Handle(NoteTitleChanged evnt)
        {
            _title = evnt.Title;
        }
    }
}
