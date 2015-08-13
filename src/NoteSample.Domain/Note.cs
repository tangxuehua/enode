using System;
using ENode.Domain;

namespace NoteSample.Domain
{
    public class Note : AggregateRoot<string>
    {
        private string _title;

        public string Title { get { return _title; } }

        public Note(string id, string title) : base(id)
        {
            ApplyEvent(new NoteCreated(this, title));
        }

        public void ChangeTitle(string title)
        {
            ApplyEvent(new NoteTitleChanged(this, title));
        }
        public void TestEvents()
        {
            ApplyEvents(new Event1(this), new Event2(this), new Event3(this));
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
        private void Handle(Event1 evnt)
        {
        }
        private void Handle(Event2 evnt)
        {
        }
        private void Handle(Event3 evnt)
        {
        }
    }
}
