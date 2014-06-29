using ECommon.Components;
using ECommon.Serializing;
using EQueue.Clients.Producers;
using EQueue.Protocols;

namespace ENode.EQueue
{
    public class DomainEventHandledMessageSender
    {
        private const string DefaultDomainEventHandledMessageSenderProcuderId = "sys_dehmsp";
        private readonly Producer _producer;
        private readonly IBinarySerializer _binarySerializer;

        public Producer Producer { get { return _producer; } }

        public DomainEventHandledMessageSender(string id = null, ProducerSetting setting = null)
        {
            _producer = new Producer(id ?? DefaultDomainEventHandledMessageSenderProcuderId, setting ?? new ProducerSetting());
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
            _producer.SendAsync(new Message(topic, _binarySerializer.Serialize(message)), message.CommandId);
        }
    }
}
