using ECommon.IoC;
using ECommon.Serializing;
using ECommon.Socketing;
using ECommon.Utilities;
using EQueue.Clients.Producers;
using EQueue.Protocols;

namespace ENode.EQueue
{
    public class DomainEventHandledMessageSender
    {
        private readonly Producer _producer;
        private readonly IBinarySerializer _binarySerializer;

        public Producer Producer { get { return _producer; } }

        public DomainEventHandledMessageSender() : this(new ProducerSetting()) { }
        public DomainEventHandledMessageSender(ProducerSetting setting) : this(null, setting) { }
        public DomainEventHandledMessageSender(string name, ProducerSetting setting) : this(setting, string.Format("{0}@{1}@{2}", SocketUtils.GetLocalIPV4(), string.IsNullOrEmpty(name) ? typeof(DomainEventHandledMessageSender).Name : name, ObjectId.GenerateNewId())) { }
        public DomainEventHandledMessageSender(ProducerSetting setting, string id)
        {
            _producer = new Producer(setting, id);
            _binarySerializer = ObjectContainer.Resolve<IBinarySerializer>();
        }

        public DomainEventHandledMessageSender Start()
        {
            _producer.Start();
            return this;
        }
        public DomainEventHandledMessageSender Shutdown()
        {
            _producer.Shutdown();
            return this;
        }
        public void Send(DomainEventHandledMessage message, string topic)
        {
            _producer.SendAsync(new Message(topic, _binarySerializer.Serialize(message)), message.AggregateRootId);
        }
    }
}
