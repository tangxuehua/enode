using System.Text;
using ECommon.Components;
using ECommon.Serializing;
using EQueue.Clients.Producers;
using EQueue.Protocols;

namespace ENode.EQueue
{
    public class DomainEventHandledMessageSender
    {
        private const string DefaultDomainEventHandledMessageSenderProcuderId = "DomainEventHandledMessageSender";
        private readonly Producer _producer;
        private readonly IJsonSerializer _jsonSerializer;

        public Producer Producer { get { return _producer; } }

        public DomainEventHandledMessageSender(string id = null, ProducerSetting setting = null)
        {
            _producer = new Producer(id ?? DefaultDomainEventHandledMessageSenderProcuderId, setting ?? new ProducerSetting());
            _jsonSerializer = ObjectContainer.Resolve<IJsonSerializer>();
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
            _producer.SendAsync(new Message(topic, (int)EQueueMessageTypeCode.DomainEventHandledMessage, Encoding.UTF8.GetBytes(_jsonSerializer.Serialize(message))), message.CommandId);
        }
    }
}
