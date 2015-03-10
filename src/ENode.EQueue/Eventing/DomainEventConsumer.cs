using System.Text;
using ECommon.Components;
using ECommon.Logging;
using ECommon.Serializing;
using ECommon.Utilities;
using ENode.Eventing;
using ENode.Infrastructure;
using EQueue.Clients.Consumers;
using EQueue.Protocols;
using IQueueMessageHandler = EQueue.Clients.Consumers.IMessageHandler;

namespace ENode.EQueue
{
    public class DomainEventConsumer : IQueueMessageHandler
    {
        private const string DefaultEventConsumerId = "EventConsumer";
        private const string DefaultEventConsumerGroup = "EventConsumerGroup";
        private readonly Consumer _consumer;
        private readonly DomainEventHandledMessageSender _domainEventHandledMessageSender;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IEventSerializer _eventSerializer;
        private readonly IMessageProcessor<ProcessingDomainEventStreamMessage, DomainEventStreamMessage, bool> _processor;
        private readonly ILogger _logger;
        private readonly bool _sendEventHandledMessage;

        public Consumer Consumer { get { return _consumer; } }

        public DomainEventConsumer(string id = null, string groupName = null, ConsumerSetting setting = null, DomainEventHandledMessageSender domainEventHandledMessageSender = null, bool sendEventHandledMessage = true)
        {
            var consumerId = id ?? DefaultEventConsumerId;
            _consumer = new Consumer(consumerId, groupName ?? DefaultEventConsumerGroup, setting ?? new ConsumerSetting
            {
                MessageHandleMode = MessageHandleMode.Sequential
            });
            _jsonSerializer = ObjectContainer.Resolve<IJsonSerializer>();
            _eventSerializer = ObjectContainer.Resolve<IEventSerializer>();
            _processor = ObjectContainer.Resolve<IMessageProcessor<ProcessingDomainEventStreamMessage, DomainEventStreamMessage, bool>>();
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().FullName);
            _domainEventHandledMessageSender = domainEventHandledMessageSender ?? new DomainEventHandledMessageSender();
            _sendEventHandledMessage = sendEventHandledMessage;
        }

        public DomainEventConsumer Start()
        {
            _consumer.SetMessageHandler(this).Start();
            if (_sendEventHandledMessage)
            {
                _domainEventHandledMessageSender.Start();
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
                _domainEventHandledMessageSender.Shutdown();
            }
            return this;
        }

        void IQueueMessageHandler.Handle(QueueMessage queueMessage, IMessageContext context)
        {
            var message = _jsonSerializer.Deserialize(Encoding.UTF8.GetString(queueMessage.Body), typeof(EventStreamMessage)) as EventStreamMessage;
            var domainEventStreamMessage = ConvertToDomainEventStream(message);
            var processContext = new DomainEventStreamProcessContext(this, domainEventStreamMessage, queueMessage, context);
            var processingMessage = new ProcessingDomainEventStreamMessage(domainEventStreamMessage, processContext);
            _processor.Process(processingMessage);
        }

        private DomainEventStreamMessage ConvertToDomainEventStream(EventStreamMessage message)
        {
            return new DomainEventStreamMessage
            {
                Id = ObjectId.GenerateNewStringId(),
                CommandId = message.CommandId,
                AggregateRootId = message.AggregateRootId,
                Version = message.Version,
                Timestamp = message.Timestamp,
                Items = message.Items,
                Events = _eventSerializer.Deserialize<IDomainEvent>(message.Events)
            };
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

                var topic = Constants.DomainEventHandledMessageTopic;
                var items = _domainEventStreamMessage.Items;
                if (!items.ContainsKey(topic) || string.IsNullOrEmpty(items[topic]))
                {
                    _eventConsumer._logger.ErrorFormat("{0} cannot be null or empty. current eventStream:{1}", topic, _domainEventStreamMessage);
                    return;
                }
                var domainEventHandledMessageTopic = items[topic];

                _eventConsumer._domainEventHandledMessageSender.Send(new DomainEventHandledMessage
                {
                    CommandId = _domainEventStreamMessage.CommandId,
                    AggregateRootId = _domainEventStreamMessage.AggregateRootId
                }, domainEventHandledMessageTopic);
            }
        }
    }
}
