using System;
using ECommon.Utilities;

namespace ENode.Messaging
{
    /// <summary>Represents an abstract base message.
    /// </summary>
    [Serializable]
    public abstract class Message : IMessage
    {
        /// <summary>Represents the identifier of the message.
        /// </summary>
        public string Id { get; set; }

        /// <summary>Default constructor.
        /// </summary>
        public Message()
        {
            Id = ObjectId.GenerateNewStringId();
        }
    }
}
