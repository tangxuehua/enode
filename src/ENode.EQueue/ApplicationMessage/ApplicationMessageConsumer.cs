using System.Text;
using System.Threading.Tasks;
using ECommon.Components;
using ECommon.Logging;
using ECommon.Serializing;
using ENode.Infrastructure;
using EQueue.Clients.Consumers;
using EQueue.Protocols;
using IQueueMessageHandler = EQueue.Clients.Consumers.IMessageHandler;

namespace ENode.EQueue
{
    public class ApplicationMessageConsumer : IQueueMessageHandler
    {
        private const string DefaultMessageConsumerGroup = "ApplicationMessageConsumerGroup";
        private IJsonSerializer _jsonSerializer;
        private ITypeNameProvider _typeNameProvider;
        private IMessageDispatcher _messageDispatcher;
        private ILogger _logger;

        public Consumer Consumer { get; private set; }

        public ApplicationMessageConsumer InitializeENode()
        {
            _jsonSerializer = ObjectContainer.Resolve<IJsonSerializer>();
            _messageDispatcher = ObjectContainer.Resolve<IMessageDispatcher>();
            _typeNameProvider = ObjectContainer.Resolve<ITypeNameProvider>();
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().FullName);
            return this;
        }
        public ApplicationMessageConsumer InitializeEQueue(string groupName = null, ConsumerSetting setting = null)
        {
            InitializeENode();
            Consumer = new Consumer(groupName ?? DefaultMessageConsumerGroup, setting ?? new ConsumerSetting
            {
                MessageHandleMode = MessageHandleMode.Sequential,
                ConsumeFromWhere = ConsumeFromWhere.FirstOffset
            }, "ApplicationMessageConsumer");
            return this;
        }

        public ApplicationMessageConsumer Start()
        {
            Consumer.SetMessageHandler(this).Start();
            return this;
        }
        public ApplicationMessageConsumer Subscribe(string topic)
        {
            Consumer.Subscribe(topic);
            return this;
        }
        public ApplicationMessageConsumer Shutdown()
        {
            Consumer.Stop();
            return this;
        }

        void IQueueMessageHandler.Handle(QueueMessage queueMessage, IMessageContext context)
        {
            var applicationMessageType = _typeNameProvider.GetType(queueMessage.Tag);
            var message = _jsonSerializer.Deserialize(Encoding.UTF8.GetString(queueMessage.Body), applicationMessageType) as IApplicationMessage;
            var processContext = new EQueueProcessContext(queueMessage, context);
            var processingMessage = new ProcessingApplicationMessage(message, processContext);
            _logger.DebugFormat("ENode application message received, messageId: {0}, routingKey: {1}", message.Id, message.GetRoutingKey());

            Task.Factory.StartNew(obj =>
            {
                _messageDispatcher.DispatchMessageAsync(((ProcessingApplicationMessage)obj).Message).ContinueWith(x =>
                {
                    processingMessage.Complete();
                });
            }, processingMessage);
        }
    }
}
