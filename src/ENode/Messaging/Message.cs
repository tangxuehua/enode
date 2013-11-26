using System;

namespace ENode.Messaging
{
    /// <summary>Represents a message.
    /// </summary>
    [Serializable]
    public class Message<TPayload> : IMessage where TPayload : class, IPayload
    {
        /// <summary>Represents the unique identifier for the message.
        /// </summary>
        public Guid Id { get; private set; }
        /// <summary>Represents the payload object of the message.
        /// </summary>
        public TPayload Payload { get; private set; }
        /// <summary>Represents which queue the message from.
        /// </summary>
        public string QueueName { get; private set; }

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="payload"></param>
        /// <param name="queueName"></param>
        public Message(Guid id, TPayload payload, string queueName)
        {
            Id = id;
            Payload = payload;
            QueueName = queueName;
        }

        /// <summary>Represents the payload object of the message.
        /// </summary>
        object IMessage.Payload
        {
            get { return this.Payload; }
        }
    }
}
