using System;

namespace ENode.Infrastructure
{
    /// <summary>Represents a message has sequence.
    /// </summary>
    public interface ISequenceMessage : IMessage
    {
        /// <summary>Represents the aggregate root id of the sequence message.
        /// </summary>
        string AggregateRootId { get; set; }
        /// <summary>Represents the aggregate root type code of the sequence message.
        /// </summary>
        string AggregateRootTypeName { get; set; }
        /// <summary>Represents the main version of the sequence message.
        /// </summary>
        int Version { get; set; }
    }
}
