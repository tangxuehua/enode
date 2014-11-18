using System;
using System.Collections.Generic;
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
        private readonly IBinarySerializer _binarySerializer;
        private readonly ITopicProvider<IMessage> _messageTopicProvider;
        private readonly ITypeCodeProvider<IMessage> _messageTypeCodeProvider;
        private readonly Producer _producer;

        public Producer Producer { get { return _producer; } }

        public MessagePublisher(string id = null, ProducerSetting setting = null)
        {
            _producer = new Producer(id ?? DefaultMessagePublisherProcuderId, setting ?? new ProducerSetting());
            _binarySerializer = ObjectContainer.Resolve<IBinarySerializer>();
            _messageTopicProvider = ObjectContainer.Resolve<ITopicProvider<IMessage>>();
            _messageTypeCodeProvider = ObjectContainer.Resolve<ITypeCodeProvider<IMessage>>();
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
            var serializableInfo = new Dictionary<string, string>();
            var data = _binarySerializer.Serialize(message);
            var queueMessage = new EQueueMessage(topic, messageTypeCode, data);
            var result = _producer.Send(queueMessage, message is IVersionedMessage ? ((IVersionedMessage)message).SourceId : message.Id);
            if (result.SendStatus != SendStatus.Success)
            {
                throw new Exception(string.Format("Publish message failed, messageId:{0}, messageType:{1}", message.Id, message.GetType().Name));
            }
        }
    }
}
