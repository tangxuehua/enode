using System.Runtime.Serialization;
using ECommon.Components;
using ECommon.Logging;
using ECommon.Serializing;
using ENode.Exceptions;
using ENode.Infrastructure;
using EQueue.Clients.Consumers;
using EQueue.Protocols;
using IQueueMessageHandler = EQueue.Clients.Consumers.IMessageHandler;

namespace ENode.EQueue
{
    public class PublishableExceptionConsumer : IQueueMessageHandler
    {
        private const string DefaultExceptionConsumerId = "ExceptionConsumer";
        private const string DefaultExceptionConsumerGroup = "ExceptionConsumerGroup";
        private readonly Consumer _consumer;
        private readonly IBinarySerializer _binarySerializer;
        private readonly ITypeCodeProvider<IPublishableException> _publishableExceptionTypeCodeProvider;
        private readonly IMessageProcessor<IPublishableException, bool> _publishableExceptionProcessor;
        private readonly ILogger _logger;

        public Consumer Consumer { get { return _consumer; } }

        public PublishableExceptionConsumer(string id = null, string groupName = null, ConsumerSetting setting = null, DomainEventHandledMessageSender domainEventHandledMessageSender = null, bool sendEventHandledMessage = true)
        {
            var consumerId = id ?? DefaultExceptionConsumerId;
            _consumer = new Consumer(consumerId, groupName ?? DefaultExceptionConsumerGroup, setting ?? new ConsumerSetting
            {
                MessageHandleMode = MessageHandleMode.Sequential
            });
            _binarySerializer = ObjectContainer.Resolve<IBinarySerializer>();
            _publishableExceptionProcessor = ObjectContainer.Resolve<IMessageProcessor<IPublishableException, bool>>();
            _publishableExceptionTypeCodeProvider = ObjectContainer.Resolve<ITypeCodeProvider<IPublishableException>>();
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().FullName);
        }

        public PublishableExceptionConsumer Start()
        {
            _consumer.SetMessageHandler(this).Start();
            return this;
        }
        public PublishableExceptionConsumer Subscribe(string topic)
        {
            _consumer.Subscribe(topic);
            return this;
        }
        public PublishableExceptionConsumer Shutdown()
        {
            _consumer.Shutdown();
            return this;
        }

        void IQueueMessageHandler.Handle(QueueMessage message, IMessageContext context)
        {
            var publishableExceptionMessage = _binarySerializer.Deserialize<PublishableExceptionMessage>(message.Body);
            var publishableExceptionType = _publishableExceptionTypeCodeProvider.GetType(publishableExceptionMessage.ExceptionTypeCode);
            var publishableException = FormatterServices.GetUninitializedObject(publishableExceptionType) as IPublishableException;
            publishableException.UniqueId = publishableExceptionMessage.UniqueId;
            publishableException.RestoreFrom(publishableExceptionMessage.SerializableInfo);
            _publishableExceptionProcessor.Process(publishableException, new PublishableExceptionProcessContext(message, context, publishableException));
        }

        class PublishableExceptionProcessContext : MessageProcessContext<IPublishableException, bool>
        {
            public PublishableExceptionProcessContext(QueueMessage queueMessage, IMessageContext messageContext, IPublishableException publishableException)
                : base(queueMessage, messageContext, publishableException)
            {
            }
        }
    }
}
