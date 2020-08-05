using System.Runtime.Serialization;
using System.Text;
using ECommon.Components;
using ECommon.Logging;
using ECommon.Serializing;
using ENode.Domain;
using ENode.Infrastructure;
using ENode.Messaging;
using EQueue.Clients.Consumers;
using EQueue.Protocols;
using IQueueMessageHandler = EQueue.Clients.Consumers.IMessageHandler;

namespace ENode.EQueue
{
    public class DomainExceptionConsumer : IQueueMessageHandler
    {
        private const string DefaultExceptionConsumerGroup = "ExceptionConsumerGroup";
        private IJsonSerializer _jsonSerializer;
        private ITypeNameProvider _typeNameProvider;
        private IMessageDispatcher _messageDispatcher;
        private ILogger _logger;

        public Consumer Consumer { get; private set; }

        public DomainExceptionConsumer InitializeENode()
        {
            _jsonSerializer = ObjectContainer.Resolve<IJsonSerializer>();
            _messageDispatcher = ObjectContainer.Resolve<IMessageDispatcher>();
            _typeNameProvider = ObjectContainer.Resolve<ITypeNameProvider>();
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().FullName);
            return this;
        }
        public DomainExceptionConsumer InitializeEQueue(string groupName = null, ConsumerSetting setting = null)
        {
            InitializeENode();
            Consumer = new Consumer(groupName ?? DefaultExceptionConsumerGroup, setting ?? new ConsumerSetting
            {
                MessageHandleMode = MessageHandleMode.Sequential,
                ConsumeFromWhere = ConsumeFromWhere.FirstOffset
            }, "DomainExceptionConsumer");
            return this;
        }

        public DomainExceptionConsumer Start()
        {
            Consumer.SetMessageHandler(this).Start();
            return this;
        }
        public DomainExceptionConsumer Subscribe(string topic)
        {
            Consumer.Subscribe(topic);
            return this;
        }
        public DomainExceptionConsumer Shutdown()
        {
            Consumer.Stop();
            return this;
        }

        void IQueueMessageHandler.Handle(QueueMessage queueMessage, IMessageContext context)
        {
            var exceptionType = _typeNameProvider.GetType(queueMessage.Tag);
            var domainExceptionString = Encoding.UTF8.GetString(queueMessage.Body);

            _logger.InfoFormat("Received domain exception equeue message: {0}, domainExceptionMessage: {1}", queueMessage, domainExceptionString);

            var exceptionMessage = _jsonSerializer.Deserialize<DomainExceptionMessage>(domainExceptionString);
            var exception = FormatterServices.GetUninitializedObject(exceptionType) as IDomainException;
            exception.Id = exceptionMessage.UniqueId;
            exception.Timestamp = exceptionMessage.Timestamp;
            exception.Items = exceptionMessage.Items;
            exception.RestoreFrom(exceptionMessage.SerializableInfo);

            _messageDispatcher.DispatchMessageAsync(exception).ContinueWith(x =>
            {
                context.OnMessageHandled(queueMessage);
            });
        }
    }
}
