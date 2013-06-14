using System.Collections.Generic;

namespace ENode.Messaging
{
    public class EmptyMessageStore : IMessageStore
    {
        public void Initialize(string queueName) { }
        public void AddMessage(string queueName, IMessage message) { }
        public void RemoveMessage(string queueName, IMessage message) { }
        public IEnumerable<T> GetMessages<T>(string queueName) where T : class, IMessage { return new T[] { }; }
    }
}
