using System;
using ENode.Eventing;

namespace NoteSample.DomainEvents
{
    [Serializable]
    public class NoteTitleChangedEvent : DomainEvent<string>
    {
        public string Title { get; private set; }

        public NoteTitleChangedEvent(string id, string title) : base(id)
        {
            Title = title;
        }
    }
}
