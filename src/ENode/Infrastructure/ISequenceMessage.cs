using System;

namespace ENode.Infrastructure
{
    /// <summary>Represents a message has sequence.
    /// </summary>
    public interface ISequenceMessage : IMessage
    {
        /// <summary>Represents the aggregate root id of the sequence message.
        /// </summary>
        string AggregateRootId { get; }
        /// <summary>Represents the aggregate root type code of the sequence message.
        /// </summary>
        int AggregateRootTypeCode { get; }
        /// <summary>Represents the version of the sequence message.
        /// </summary>
        int Version { get; }
        /// <summary>Set the aggregate root type code of the sequence message.
        /// </summary>
        /// <param name="aggregateRootTypeCode"></param>
        void SetAggregateRootTypeCode(int aggregateRootTypeCode);
        /// <summary>Set the aggregate root id of the sequence message.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        void SetAggregateRootId(string aggregateRootId);
    }
}
