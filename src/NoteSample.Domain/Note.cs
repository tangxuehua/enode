using System;
using ECommon.Components;
using ECommon.Logging;
using ENode.Domain;
using ENode.Infrastructure;

namespace NoteSample.Domain
{
    public class Note : AggregateRoot<string>
    {
        private string _title;
        private static ILogger _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(typeof(Note).Name);

        public string Title { get { return _title; } }

        public Note(string id, string title) : base(id)
        {
            ApplyEvent(new NoteCreated(this, title));
        }

        public string ChangeTitle(string title)
        {
            ApplyEvent(new NoteTitleChanged(this, title));

            return "";
        }
        public void TestEvents()
        {
            ApplyEvents(new Event1(this), new Event2(this), new Event3(this));
        }

        private void Handle(NoteCreated evnt)
        {
            _id = evnt.AggregateRootId;
            _title = evnt.Title;
            if (_logger.IsDebugEnabled)
            {
                _logger.DebugFormat("Note created, title: {0}", _title);
            }
        }
        private void Handle(NoteTitleChanged evnt)
        {
            _title = evnt.Title;
            if (_logger.IsDebugEnabled)
            {
                _logger.DebugFormat("Note updated, title: {0}", _title);
            }
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
