using System;
using ENode.Eventing;

namespace NoteSample.DomainEvents
{
    [Serializable]
    public class NoteTitleChanged : DomainEvent<string>
    {
        public string Title { get; private set; }

        public NoteTitleChanged(string id, string title) : base(id)
        {
            Title = title;
        }
    }
}
