using System;

namespace ENode.Eventing
{
    /// <summary>Represents a base domain event.
    /// </summary>
    [Serializable]
    public class Event : IEvent
    {
        /// <summary>Parameterized constructor.
        /// </summary>
        public Event(object sourceId)
        {
            Id = Guid.NewGuid();
            if (sourceId == null)
            {
                throw new ArgumentException("SourceId cannot be null.");
            }
            SourceId = sourceId;
        }

        /// <summary>Represents the unique identifier for the event.
        /// </summary>
        public Guid Id { get; private set; }
        /// <summary>Represents the source of the event, which means which aggregate raised this event.
        /// </summary>
        public object SourceId { get; private set; }
    }
}
