using System;

namespace ENode.Messaging
{
    /// <summary>Represents a payload object.
    /// </summary>
    public interface IPayload
    {
        /// <summary>Represents the unique identifier of the payload object.
        /// </summary>
        Guid Id { get; }
    }
}
