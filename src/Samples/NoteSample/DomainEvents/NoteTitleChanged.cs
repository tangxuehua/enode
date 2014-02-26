using System;
using ENode.Eventing;
using NoteSample.Domain;

namespace NoteSample.DomainEvents
{
    [Serializable]
    public class NoteTitleChangedEvent : DomainEvent<Guid>
    {
        public string Title { get; private set; }

        public NoteTitleChangedEvent(Guid id, string title) : base(id)
        {
            Title = title;
        }
    }
}
