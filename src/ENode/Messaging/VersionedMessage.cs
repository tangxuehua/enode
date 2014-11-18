using System;

namespace ENode.Messaging
{
    /// <summary>Represents an abstract base domain event.
    /// </summary>
    [Serializable]
    public abstract class VersionedMessage<TSourceId> : Message, IVersionedMessage
    {
        /// <summary>Represents the identifier of the source which originating the message.
        /// </summary>
        public TSourceId SourceId { get; set; }
        /// <summary>Represents the version of the message.
        /// </summary>
        public int Version { get; set; }
        /// <summary>Represents the occurred time of the message.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>Default constructor.
        /// </summary>
        public VersionedMessage() : base() { }
        /// <summary>Parameterized constructor.
        /// </summary>
        public VersionedMessage(TSourceId sourceId) : base()
        {
            if (sourceId == null)
            {
                throw new ArgumentNullException("sourceId");
            }
            SourceId = sourceId;
        }

        string IVersionedMessage.SourceId
        {
            get
            {
                if (this.SourceId != null)
                {
                    return this.SourceId.ToString();
                }
                return null;
            }
        }
    }
}
