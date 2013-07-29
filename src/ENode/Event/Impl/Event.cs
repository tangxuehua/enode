using System;
using ENode.Messaging;

namespace ENode.Eventing
{
    /// <summary>Represents a base domain event.
    /// </summary>
    [Serializable]
    public class Event: Message, IEvent
    {
        public Event() : base(Guid.NewGuid()) { }
    }
}
