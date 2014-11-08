using System;
using ECommon.Utilities;

namespace ENode.Eventing
{
    /// <summary>Represents an abstract base event.
    /// </summary>
    [Serializable]
    public abstract class Event : IEvent
    {
        /// <summary>Represents the unique id of the event.
        /// </summary>
        public string Id { get; set; }

        /// <summary>Default constructor.
        /// </summary>
        public Event()
        {
            Id = ObjectId.GenerateNewStringId();
        }
    }
}
