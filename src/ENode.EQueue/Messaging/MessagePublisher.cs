using System;
using System.Text;
using ECommon.Components;
using ECommon.Logging;
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
        private readonly ILogger _logger;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ITopicProvider<IMessage> _messageTopicProvider;
        private readonly ITypeCodeProvider<IMessage> _messageTypeCodeProvider;
        private readonly Producer _producer;
        private readonly IOHelper _ioHelper;

        public Producer Producer { get { return _producer; } }

        public MessagePublisher(string id = null, ProducerSetting setting = null)
        {
            _producer = new Producer(id ?? DefaultMessagePublisherProcuderId, setting ?? new ProducerSetting());
            _jsonSerializer = ObjectContainer.Resolve<IJsonSerializer>();
            _messageTopicProvider = ObjectContainer.Resolve<ITopicProvider<IMessage>>();
            _messageTypeCodeProvider = ObjectContainer.Resolve<ITypeCodeProvider<IMessage>>();
            _ioHelper = ObjectContainer.Resolve<IOHelper>();
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().FullName);
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
            var messageTypeCode = _messageTypeCodeProvider.GetTypeCode(message.GetType());
            var topic = _messageTopicProvider.GetTopic(message);
            var data = _jsonSerializer.Serialize(message);
            var queueMessage = new EQueueMessage(topic, messageTypeCode, Encoding.UTF8.GetBytes(data));
            _ioHelper.TryIOAction(() =>
            {
                var result = _producer.Send(queueMessage, message is IVersionedMessage ? ((IVersionedMessage)message).SourceId : message.Id);
                if (result.SendStatus != SendStatus.Success)
                {
                    throw new Exception(string.Format("Publish message failed, messageId:{0}, messageType:{1}", message.Id, message.GetType().Name));
                }
            }, "Send message to broker");
        }
    }
}
