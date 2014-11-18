using System;

namespace ENode.Messaging
{
    /// <summary>Represents a versioned message.
    /// </summary>
    public interface IVersionedMessage : IMessage
    {
        /// <summary>Represents the identifier of the source which originating the message.
        /// </summary>
        string SourceId { get; }
        /// <summary>Represents the version of the message.
        /// </summary>
        int Version { get; }
        /// <summary>Represents the occurred time of the message.
        /// </summary>
        DateTime Timestamp { get; }
    }
}
