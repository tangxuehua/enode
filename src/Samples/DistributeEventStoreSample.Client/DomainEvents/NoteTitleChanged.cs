using System;
using ENode.Eventing;

namespace DistributeEventStoreSample.Client.DomainEvents
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
