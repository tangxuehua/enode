using System;

namespace ENode.Eventing
{
    /// <summary>Represents a base domain event.
    /// </summary>
    [Serializable]
    public class Event: IEvent
    {
        /// <summary>The unique identifier of the domain event.
        /// </summary>
        public Guid Id { get; private set; }

        public Event()
        {
            Id = Guid.NewGuid();
        }
    }
}
