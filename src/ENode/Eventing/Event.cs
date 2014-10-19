using System;
using ECommon.Utilities;

namespace ENode.Eventing
{
    /// <summary>Represents an abstract base event.
    /// </summary>
    [Serializable]
    public abstract class Event : IEvent
    {
        /// <summary>Default constructor.
        /// </summary>
        public Event()
        {
            Id = ObjectId.GenerateNewStringId();
        }

        /// <summary>Represents the unique id of the event.
        /// </summary>
        public string Id { get; private set; }
        /// <summary>Represents the occurred time of the domain event.
        /// </summary>
        public DateTime Timestamp { get; private set; }

        DateTime IEvent.Timestamp
        {
            get
            {
                return this.Timestamp;
            }
            set
            {
                Timestamp = value;
            }
        }
    }
}
