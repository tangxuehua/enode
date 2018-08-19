using System.Runtime.Serialization;
using System.Text;
using ECommon.Components;
using ECommon.Logging;
using ECommon.Serializing;
using ENode.Infrastructure;
using EQueue.Clients.Consumers;
using EQueue.Protocols;
using IQueueMessageHandler = EQueue.Clients.Consumers.IMessageHandler;

namespace ENode.EQueue
{
    public class PublishableExceptionConsumer : IQueueMessageHandler
    {
        private const string DefaultExceptionConsumerGroup = "ExceptionConsumerGroup";
        private IJsonSerializer _jsonSerializer;
        private ITypeNameProvider _typeNameProvider;
        private IMessageProcessor<ProcessingPublishableExceptionMessage, IPublishableException> _publishableExceptionProcessor;
        private ILogger _logger;

        public Consumer Consumer { get; private set; }

        public PublishableExceptionConsumer InitializeENode()
        {
            _jsonSerializer = ObjectContainer.Resolve<IJsonSerializer>();
            _publishableExceptionProcessor = ObjectContainer.Resolve<IMessageProcessor<ProcessingPublishableExceptionMessage, IPublishableException>>();
            _typeNameProvider = ObjectContainer.Resolve<ITypeNameProvider>();
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().FullName);
            return this;
        }
        public PublishableExceptionConsumer InitializeEQueue(string groupName = null, ConsumerSetting setting = null)
        {
            InitializeENode();
            Consumer = new Consumer(groupName ?? DefaultExceptionConsumerGroup, setting ?? new ConsumerSetting
            {
                MessageHandleMode = MessageHandleMode.Sequential,
                ConsumeFromWhere = ConsumeFromWhere.FirstOffset
            }, "PublishableExceptionConsumer");
            return this;
        }

        public PublishableExceptionConsumer Start()
        {
            Consumer.SetMessageHandler(this).Start();
            return this;
        }
        public PublishableExceptionConsumer Subscribe(string topic)
        {
            Consumer.Subscribe(topic);
            return this;
        }
        public PublishableExceptionConsumer Shutdown()
        {
            Consumer.Stop();
            return this;
        }

        void IQueueMessageHandler.Handle(QueueMessage queueMessage, IMessageContext context)
        {
            var exceptionMessage = _jsonSerializer.Deserialize<PublishableExceptionMessage>(Encoding.UTF8.GetString(queueMessage.Body));
            var exceptionType = _typeNameProvider.GetType(queueMessage.Tag);
            var exception = FormatterServices.GetUninitializedObject(exceptionType) as IPublishableException;
            exception.Id = exceptionMessage.UniqueId;
            exception.Timestamp = exceptionMessage.Timestamp;
            exception.RestoreFrom(exceptionMessage.SerializableInfo);
            var sequenceMessage = exception as ISequenceMessage;
            if (sequenceMessage != null)
            {
                sequenceMessage.AggregateRootTypeName = exceptionMessage.AggregateRootTypeName;
                sequenceMessage.AggregateRootStringId = exceptionMessage.AggregateRootId;
            }
            var processContext = new EQueueProcessContext(queueMessage, context);
            var processingMessage = new ProcessingPublishableExceptionMessage(exception, processContext);
            _logger.InfoFormat("ENode exception message received, messageId: {0}, aggregateRootId: {1}, aggregateRootType: {2}", exceptionMessage.UniqueId, exceptionMessage.AggregateRootId, exceptionMessage.AggregateRootTypeName);
            _publishableExceptionProcessor.Process(processingMessage);
        }
    }
}
