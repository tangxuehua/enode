using System.Text;
using System.Threading.Tasks;
using ECommon.Components;
using ECommon.IO;
using ECommon.Serializing;
using ENode.Infrastructure;
using EQueue.Clients.Producers;
using EQueueMessage = EQueue.Protocols.Message;

namespace ENode.EQueue
{
    public class ApplicationMessagePublisher : IMessagePublisher<IApplicationMessage>
    {
        private const string DefaultMessagePublisherProcuderId = "ApplicationMessagePublisher";
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ITopicProvider<IApplicationMessage> _messageTopicProvider;
        private readonly ITypeCodeProvider _messageTypeCodeProvider;
        private readonly Producer _producer;
        private readonly SendQueueMessageService _sendMessageService;

        public Producer Producer { get { return _producer; } }

        public ApplicationMessagePublisher(string id = null, ProducerSetting setting = null)
        {
            _producer = new Producer(id ?? DefaultMessagePublisherProcuderId, setting ?? new ProducerSetting());
            _jsonSerializer = ObjectContainer.Resolve<IJsonSerializer>();
            _messageTopicProvider = ObjectContainer.Resolve<ITopicProvider<IApplicationMessage>>();
            _messageTypeCodeProvider = ObjectContainer.Resolve<ITypeCodeProvider>();
            _sendMessageService = new SendQueueMessageService();
        }

        public ApplicationMessagePublisher Start()
        {
            _producer.Start();
            return this;
        }
        public ApplicationMessagePublisher Shutdown()
        {
            _producer.Shutdown();
            return this;
        }

        public Task<AsyncTaskResult> PublishAsync(IApplicationMessage message)
        {
            var queueMessage = CreateEQueueMessage(message);
            return _sendMessageService.SendMessageAsync(_producer, queueMessage, message.GetRoutingKey() ?? message.Id);
        }

        private EQueueMessage CreateEQueueMessage(IApplicationMessage message)
        {
            var messageTypeCode = _messageTypeCodeProvider.GetTypeCode(message.GetType());
            var topic = _messageTopicProvider.GetTopic(message);
            var data = _jsonSerializer.Serialize(message);
            return new EQueueMessage(topic, messageTypeCode, Encoding.UTF8.GetBytes(data));
        }
    }
}
