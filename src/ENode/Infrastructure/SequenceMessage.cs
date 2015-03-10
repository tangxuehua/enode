using System;

namespace ENode.Infrastructure
{
    /// <summary>Represents an abstract sequence message.
    /// </summary>
    [Serializable]
    public abstract class SequenceMessage<TAggregateRootId> : Message, ISequenceMessage
    {
        /// <summary>Represents the aggregate root id of the sequence message.
        /// </summary>
        public TAggregateRootId AggregateRootId { get; set; }
        /// <summary>Represents the version of the sequence message.
        /// </summary>
        public int Version { get; set; }

        /// <summary>Default constructor.
        /// </summary>
        public SequenceMessage() : base() { }
        /// <summary>Parameterized constructor.
        /// </summary>
        public SequenceMessage(TAggregateRootId aggregateRootId, int version) : base()
        {
            if (aggregateRootId == null)
            {
                throw new ArgumentNullException("aggregateRootId");
            }
            if (version <= 0)
            {
                throw new ArgumentException("version cannot be small or equal than zero.");
            }
            AggregateRootId = aggregateRootId;
            Version = version;
        }

        string ISequenceMessage.AggregateRootId
        {
            get
            {
                if (this.AggregateRootId != null)
                {
                    return this.AggregateRootId.ToString();
                }
                return null;
            }
        }

        /// <summary>Returns the aggregate root id by default.
        /// </summary>
        /// <returns></returns>
        public override string GetRoutingKey()
        {
            if (!object.Equals(AggregateRootId, default(TAggregateRootId)))
            {
                return ((ISequenceMessage)this).AggregateRootId;
            }
            return null;
        }
    }
}
