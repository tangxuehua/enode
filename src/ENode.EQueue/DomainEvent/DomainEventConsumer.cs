using System.Text;
using ECommon.Components;
using ECommon.Logging;
using ECommon.Serializing;
using ENode.Eventing;
using ENode.Infrastructure;
using EQueue.Clients.Consumers;
using EQueue.Protocols;
using IQueueMessageHandler = EQueue.Clients.Consumers.IMessageHandler;

namespace ENode.EQueue
{
    public class DomainEventConsumer : IQueueMessageHandler
    {
        private const string DefaultEventConsumerGroup = "EventConsumerGroup";
        private readonly Consumer _consumer;
        private readonly SendReplyService _sendReplyService;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IEventSerializer _eventSerializer;
        private readonly IMessageProcessor<ProcessingDomainEventStreamMessage, DomainEventStreamMessage, bool> _processor;
        private readonly ILogger _logger;
        private readonly bool _sendEventHandledMessage;

        public Consumer Consumer { get { return _consumer; } }

        public DomainEventConsumer(string groupName = null, ConsumerSetting setting = null, bool sendEventHandledMessage = true)
        {
            _consumer = new Consumer(groupName ?? DefaultEventConsumerGroup, setting ?? new ConsumerSetting
            {
                MessageHandleMode = MessageHandleMode.Sequential
            });
            _sendReplyService = new SendReplyService();
            _jsonSerializer = ObjectContainer.Resolve<IJsonSerializer>();
            _eventSerializer = ObjectContainer.Resolve<IEventSerializer>();
            _processor = ObjectContainer.Resolve<IMessageProcessor<ProcessingDomainEventStreamMessage, DomainEventStreamMessage, bool>>();
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().FullName);
            _sendEventHandledMessage = sendEventHandledMessage;
        }

        public DomainEventConsumer Start()
        {
            _consumer.SetMessageHandler(this).Start();
            if (_sendEventHandledMessage)
            {
                _sendReplyService.Start();
            }
            return this;
        }
        public DomainEventConsumer Subscribe(string topic)
        {
            _consumer.Subscribe(topic);
            return this;
        }
        public DomainEventConsumer Shutdown()
        {
            _consumer.Shutdown();
            if (_sendEventHandledMessage)
            {
                _sendReplyService.Stop();
            }
            return this;
        }

        void IQueueMessageHandler.Handle(QueueMessage queueMessage, IMessageContext context)
        {
            var message = _jsonSerializer.Deserialize<EventStreamMessage>(Encoding.UTF8.GetString(queueMessage.Body));
            var domainEventStreamMessage = ConvertToDomainEventStream(message);
            var processContext = new DomainEventStreamProcessContext(this, domainEventStreamMessage, queueMessage, context);
            var processingMessage = new ProcessingDomainEventStreamMessage(domainEventStreamMessage, processContext);
            _processor.Process(processingMessage);
        }

        private DomainEventStreamMessage ConvertToDomainEventStream(EventStreamMessage message)
        {
            var domainEventStreamMessage = new DomainEventStreamMessage(
                message.CommandId,
                message.AggregateRootId,
                message.Version,
                message.AggregateRootTypeCode,
                _eventSerializer.Deserialize<IDomainEvent>(message.Events),
                message.Items);
            domainEventStreamMessage.Timestamp = message.Timestamp;
            return domainEventStreamMessage;
        }

        class DomainEventStreamProcessContext : EQueueProcessContext
        {
            private readonly DomainEventConsumer _eventConsumer;
            private readonly DomainEventStreamMessage _domainEventStreamMessage;

            public DomainEventStreamProcessContext(DomainEventConsumer eventConsumer, DomainEventStreamMessage domainEventStreamMessage, QueueMessage queueMessage, IMessageContext messageContext)
                : base(queueMessage, messageContext)
            {
                _eventConsumer = eventConsumer;
                _domainEventStreamMessage = domainEventStreamMessage;
            }

            public override void NotifyMessageProcessed()
            {
                base.NotifyMessageProcessed();

                if (!_eventConsumer._sendEventHandledMessage)
                {
                    return;
                }

                string replyAddress;
                if (!_domainEventStreamMessage.Items.TryGetValue("CommandReplyAddress", out replyAddress) || string.IsNullOrEmpty(replyAddress))
                {
                    return;
                }

                _eventConsumer._sendReplyService.SendReply((int)CommandReplyType.DomainEventHandled, new DomainEventHandledMessage
                {
                    CommandId = _domainEventStreamMessage.CommandId,
                    AggregateRootId = _domainEventStreamMessage.AggregateRootId
                }, replyAddress);
            }
        }
    }
}
