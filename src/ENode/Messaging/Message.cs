using System;

namespace ENode.Messaging
{
    /// <summary>Represents a message.
    /// </summary>
    [Serializable]
    public class Message : IMessage
    {
        [NonSerialized]
        private bool _isRestoreFromStorage = false;

        /// <summary>Represents the unique identifier for the message.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>Parameterized constructor
        /// </summary>
        /// <param name="id"></param>
        public Message(Guid id)
        {
            Id = id;
        }

        /// <summary>Returns whether the message is restore from the message store.
        /// </summary>
        public bool IsRestoreFromStorage()
        {
            return _isRestoreFromStorage;
        }
        /// <summary>Mark the message that is restored from storage.
        /// </summary>
        public void MarkAsRestoreFromStorage()
        {
            _isRestoreFromStorage = true;
        }
    }
}
