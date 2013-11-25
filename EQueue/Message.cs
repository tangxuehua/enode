using System;

namespace EQueue
{
    [Serializable]
    public class Message
    {
        public Guid Id { get; private set; }
        public byte[] Body { get; private set; }
        public string Topic { get; private set; }

        public Message(Guid id, byte[] body, string topic)
        {
            Id = id;
            Body = body;
            Topic = topic;
        }
    }
}
