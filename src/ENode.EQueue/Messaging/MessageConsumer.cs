using System.Text;
using ECommon.Components;
using ECommon.Serializing;
using ENode.Infrastructure;
using ENode.Messaging;
using EQueue.Clients.Consumers;
using EQueue.Protocols;
using IQueueMessageHandler = EQueue.Clients.Consumers.IMessageHandler;

namespace ENode.EQueue
{
    public class MessageConsumer : IQueueMessageHandler
    {
        private const string DefaultMessageConsumerId = "MessageConsumer";
        private const string DefaultMessageConsumerGroup = "MessageConsumerGroup";
        private readonly Consumer _consumer;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ITypeCodeProvider<IMessage> _messageTypeCodeProvider;
        private readonly IProcessor<IMessage> _messageProcessor;

        public Consumer Consumer { get { return _consumer; } }

        public MessageConsumer(string id = null, string groupName = null, ConsumerSetting setting = null)
        {
            var consumerId = id ?? DefaultMessageConsumerId;
            _consumer = new Consumer(consumerId, groupName ?? DefaultMessageConsumerGroup, setting ?? new ConsumerSetting
            {
                MessageHandleMode = MessageHandleMode.Sequential
            });
            _jsonSerializer = ObjectContainer.Resolve<IJsonSerializer>();
            _messageProcessor = ObjectContainer.Resolve<IProcessor<IMessage>>();
            _messageTypeCodeProvider = ObjectContainer.Resolve<ITypeCodeProvider<IMessage>>();
        }

        public MessageConsumer Start()
        {
            _consumer.SetMessageHandler(this).Start();
            return this;
        }
        public MessageConsumer Subscribe(string topic)
        {
            _consumer.Subscribe(topic);
            return this;
        }
        public MessageConsumer Shutdown()
        {
            _consumer.Shutdown();
            return this;
        }

        void IQueueMessageHandler.Handle(QueueMessage queueMessage, IMessageContext context)
        {
            var messageType = _messageTypeCodeProvider.GetType(queueMessage.Code);
            var message = _jsonSerializer.Deserialize(Encoding.UTF8.GetString(queueMessage.Body), messageType) as IMessage;
            _messageProcessor.Process(message, new EQueueProcessContext<IMessage>(queueMessage, context, message));
        }
    }
}
