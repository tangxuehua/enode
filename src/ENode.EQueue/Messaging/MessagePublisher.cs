using System.Text;
using System.Threading.Tasks;
using ECommon.Components;
using ECommon.Serializing;
using ENode.Infrastructure;
using ENode.Messaging;
using EQueue.Clients.Producers;
using EQueueMessage = EQueue.Protocols.Message;

namespace ENode.EQueue
{
    public class MessagePublisher : IPublisher<IMessage>
    {
        private const string DefaultMessagePublisherProcuderId = "MessagePublisher";
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ITopicProvider<IMessage> _messageTopicProvider;
        private readonly ITypeCodeProvider<IMessage> _messageTypeCodeProvider;
        private readonly Producer _producer;
        private readonly PublisherHelper _publisherHelper;

        public Producer Producer { get { return _producer; } }

        public MessagePublisher(string id = null, ProducerSetting setting = null)
        {
            _producer = new Producer(id ?? DefaultMessagePublisherProcuderId, setting ?? new ProducerSetting());
            _jsonSerializer = ObjectContainer.Resolve<IJsonSerializer>();
            _messageTopicProvider = ObjectContainer.Resolve<ITopicProvider<IMessage>>();
            _messageTypeCodeProvider = ObjectContainer.Resolve<ITypeCodeProvider<IMessage>>();
            _publisherHelper = new PublisherHelper();
        }

        public MessagePublisher Start()
        {
            _producer.Start();
            return this;
        }
        public MessagePublisher Shutdown()
        {
            _producer.Shutdown();
            return this;
        }

        public void Publish(IMessage message)
        {
            var queueMessage = CreateEQueueMessage(message);
            var routingKey = message is IVersionedMessage ? ((IVersionedMessage)message).SourceId : message.Id;
            _publisherHelper.PublishQueueMessage(_producer, queueMessage, routingKey, "Send message to broker");
        }
        public Task<AsyncOperationResult> PublishAsync(IMessage message)
        {
            var queueMessage = CreateEQueueMessage(message);
            var routingKey = message is IVersionedMessage ? ((IVersionedMessage)message).SourceId : message.Id;
            return _publisherHelper.PublishQueueMessageAsync(_producer, queueMessage, routingKey, "Send message to broker");
        }

        private EQueueMessage CreateEQueueMessage(IMessage message)
        {
            var messageTypeCode = _messageTypeCodeProvider.GetTypeCode(message.GetType());
            var topic = _messageTopicProvider.GetTopic(message);
            var data = _jsonSerializer.Serialize(message);
            return new EQueueMessage(topic, messageTypeCode, Encoding.UTF8.GetBytes(data));
        }
    }
}
