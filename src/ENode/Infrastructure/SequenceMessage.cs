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
        /// <summary>Represents the aggregate root string id of the sequence message.
        /// </summary>
        public string AggregateRootStringId { get; set; }
        /// <summary>Represents the aggregte root type code of the sequence message.
        /// </summary>
        public int AggregateRootTypeCode { get; set; }
        /// <summary>Represents the version of the sequence message.
        /// </summary>
        public int Version { get; set; }

        /// <summary>Default constructor.
        /// </summary>
        public SequenceMessage() : base() { }
        /// <summary>Parameterized constructor.
        /// </summary>
        public SequenceMessage(TAggregateRootId aggregateRootId, int version, int aggregateRootTypeCode = 0) : base()
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
            AggregateRootTypeCode = aggregateRootTypeCode;
            Version = version;
            if (!object.Equals(aggregateRootId, default(TAggregateRootId)))
            {
                AggregateRootStringId = aggregateRootId.ToString();
            }
        }

        string ISequenceMessage.AggregateRootId
        {
            get
            {
                return AggregateRootStringId;
            }
        }
        void ISequenceMessage.SetAggregateRootTypeCode(int aggregateRootTypeCode)
        {
            AggregateRootTypeCode = aggregateRootTypeCode;
        }
        void ISequenceMessage.SetAggregateRootId(string aggregateRootId)
        {
            AggregateRootStringId = aggregateRootId;
        }

        /// <summary>Returns the aggregate root id by default.
        /// </summary>
        /// <returns></returns>
        public override string GetRoutingKey()
        {
            return AggregateRootStringId;
        }
    }
}
