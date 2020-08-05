using System.Text;
using System.Threading.Tasks;
using ECommon.Components;
using ECommon.IO;
using ECommon.Serializing;
using ENode.Infrastructure;
using ENode.Messaging;
using EQueue.Clients.Producers;
using EQueueMessage = EQueue.Protocols.Message;

namespace ENode.EQueue
{
    public class ApplicationMessagePublisher : IMessagePublisher<IApplicationMessage>
    {
        private IJsonSerializer _jsonSerializer;
        private ITopicProvider<IApplicationMessage> _messageTopicProvider;
        private ITypeNameProvider _typeNameProvider;
        private SendQueueMessageService _sendMessageService;

        public Producer Producer { get; private set; }

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
            Producer = new Producer(setting, "ApplicationMessagePublisher");
            return this;
        }

        public ApplicationMessagePublisher Start()
        {
            Producer.Start();
            return this;
        }
        public ApplicationMessagePublisher Shutdown()
        {
            Producer.Shutdown();
            return this;
        }
        public Task PublishAsync(IApplicationMessage message)
        {
            var topic = _messageTopicProvider.GetTopic(message);
            var data = _jsonSerializer.Serialize(message);
            var equeueMessage = new EQueueMessage(
                topic,
                (int)EQueueMessageTypeCode.ApplicationMessage,
                Encoding.UTF8.GetBytes(data),
                _typeNameProvider.GetTypeName(message.GetType()));

            return _sendMessageService.SendMessageAsync(Producer, "applicationMessage", message.GetType().Name, equeueMessage, data, message.Id, message.Id, message.Items);
        }
    }
}
