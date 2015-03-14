using System;
using ECommon.Utilities;

namespace ENode.Infrastructure
{
    /// <summary>Represents an abstract message.
    /// </summary>
    [Serializable]
    public abstract class Message : IMessage
    {
        /// <summary>Represents the identifier of the message.
        /// </summary>
        public string Id { get; set; }
        /// <summary>Represents the timestamp of the message.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>Default constructor.
        /// </summary>
        public Message()
        {
            Id = ObjectId.GenerateNewStringId();
            Timestamp = DateTime.Now;
        }

        /// <summary>Returns null by default.
        /// </summary>
        /// <returns></returns>
        public virtual string GetRoutingKey()
        {
            return null;
        }

        void IMessage.SetId(string id)
        {
            Id = id;
        }
        void IMessage.SetTimestamp(DateTime timestamp)
        {
            Timestamp = timestamp;
        }
    }
}
