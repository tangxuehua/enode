using System;

namespace ENode.Messaging
{
    /// <summary>Represents a message.
    /// </summary>
    [Serializable]
    public class Message : IMessage
    {
        /// <summary>Represents the unique identifier for the message.
        /// </summary>
        public Guid Id { get; private set; }
        /// <summary>Get or set whether the message is restore from the message store.
        /// </summary>
        public bool IsRestoreFromStorage { get; set; }

        public Message(Guid id)
        {
            Id = id;
        }
    }
}
