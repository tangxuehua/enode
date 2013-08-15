using System;

namespace ENode.Messaging
{
    /// <summary>Represents a message.
    /// </summary>
    public interface IMessage
    {
        /// <summary>Represents the unique identifier for the message.
        /// </summary>
        Guid Id { get; }
        /// <summary>Returns whether the message is restore from the message store.
        /// </summary>
        bool IsRestoreFromStorage();
        /// <summary>Mark the message that is restored from storage.
        /// </summary>
        void MarkAsRestoreFromStorage();
    }
}
