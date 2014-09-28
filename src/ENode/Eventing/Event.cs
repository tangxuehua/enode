using System;
using ECommon.Utilities;
using ENode.Infrastructure;

namespace ENode.Eventing
{
    /// <summary>Represents an abstract base event.
    /// </summary>
    [Serializable]
    public abstract class Event : IEvent
    {
        private DateTime? _timestamp;

        /// <summary>Default constructor.
        /// </summary>
        public Event()
        {
            Id = ObjectId.GenerateNewStringId();
        }

        /// <summary>Represents the unique id of the event.
        /// </summary>
        public string Id { get; private set; }
        /// <summary>Represents the time of when this event raised.
        /// </summary>
        public DateTime Timestamp
        {
            get
            {
                return _timestamp == null ? DateTime.Now : _timestamp.Value;
            }
            set
            {
                if (_timestamp != null)
                {
                    throw new ENodeException("The timestamp of event cannot be set twice.");
                }
                _timestamp = value;
            }
        }
    }
}
