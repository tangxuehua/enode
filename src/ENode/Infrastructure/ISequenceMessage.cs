using System;

namespace ENode.Infrastructure
{
    /// <summary>Represents a message has sequence.
    /// </summary>
    public interface ISequenceMessage : IMessage
    {
        /// <summary>Represents the aggregate root id of the message.
        /// </summary>
        string AggregateRootId { get; }
        /// <summary>Represents the version of the message.
        /// </summary>
        int Version { get; }
    }
}
