using System;

namespace ENode.Messaging
{
    /// <summary>Represents a message.
    /// </summary>
    public interface IMessage
    {
        /// <summary>Represents the identifier of the message.
        /// </summary>
        string Id { get; }
    }
}
