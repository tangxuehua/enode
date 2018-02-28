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
        private IJsonSerializer _jsonSerializer;
        private ITopicProvider<IApplicationMessage> _messageTopicProvider;
        private ITypeNameProvider _typeNameProvider;
        private Producer _producer;
        private SendQueueMessageService _sendMessageService;

        public Producer Producer { get { return _producer; } }

        public ApplicationMessagePublisher InitializeENode()
        {
            _jsonSerializer = ObjectContainer.Resolve<IJsonSerializer>();
            _messageTopicProvider = ObjectContainer.Resolve<ITopicProvider<IApplicationMessage>>();
            _typeNameProvider = ObjectContainer.Resolve<ITypeNameProvider>();
            _sendMessageService = new SendQueueMessageService();
            return this;
        }
        public ApplicationMessagePublisher InitializeEQueue(ProducerSetting setting = null)
        {
            InitializeENode();
            _producer = new Producer(setting);
            return this;
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
            return _sendMessageService.SendMessageAsync(_producer, queueMessage, message.GetRoutingKey() ?? message.Id, message.Id, null);
        }

        private EQueueMessage CreateEQueueMessage(IApplicationMessage message)
        {
            var topic = _messageTopicProvider.GetTopic(message);
            var data = _jsonSerializer.Serialize(message);
            return new EQueueMessage(
                topic,
                (int)EQueueMessageTypeCode.ApplicationMessage,
                Encoding.UTF8.GetBytes(data),
                _typeNameProvider.GetTypeName(message.GetType()));
        }
    }
}
