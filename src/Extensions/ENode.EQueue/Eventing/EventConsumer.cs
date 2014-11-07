using System.Collections.Generic;
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
    public class EventConsumer : IQueueMessageHandler
    {
        private const string DefaultEventConsumerId = "EventConsumer";
        private const string DefaultEventConsumerGroup = "EventConsumerGroup";
        private readonly Consumer _consumer;
        private readonly DomainEventHandledMessageSender _domainEventHandledMessageSender;
        private readonly IBinarySerializer _binarySerializer;
        private readonly IMessageProcessor<IDomainEventStream> _domainEventStreamProcessor;
        private readonly IMessageProcessor<IEventStream> _eventStreamProcessor;
        private readonly IMessageProcessor<IEvent> _eventProcessor;
        private readonly ILogger _logger;
        private readonly bool _sendEventHandledMessage;

        public Consumer Consumer { get { return _consumer; } }

        public EventConsumer(string id = null, string groupName = null, ConsumerSetting setting = null, DomainEventHandledMessageSender domainEventHandledMessageSender = null, bool sendEventHandledMessage = true)
        {
            var consumerId = id ?? DefaultEventConsumerId;
            _consumer = new Consumer(consumerId, groupName ?? DefaultEventConsumerGroup, setting ?? new ConsumerSetting
            {
                MessageHandleMode = MessageHandleMode.Sequential
            });
            _binarySerializer = ObjectContainer.Resolve<IBinarySerializer>();
            _domainEventStreamProcessor = ObjectContainer.Resolve<IMessageProcessor<IDomainEventStream>>();
            _eventStreamProcessor = ObjectContainer.Resolve<IMessageProcessor<IEventStream>>();
            _eventProcessor = ObjectContainer.Resolve<IMessageProcessor<IEvent>>();
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().FullName);
            _domainEventHandledMessageSender = domainEventHandledMessageSender ?? new DomainEventHandledMessageSender();
            _eventStreamProcessor.Name = consumerId;
            _sendEventHandledMessage = sendEventHandledMessage;
        }

        public EventConsumer Start()
        {
            _consumer.SetMessageHandler(this).Start();
            _domainEventHandledMessageSender.Start();
            return this;
        }
        public EventConsumer Subscribe(string topic)
        {
            _consumer.Subscribe(topic);
            return this;
        }
        public EventConsumer Shutdown()
        {
            _consumer.Shutdown();
            _domainEventHandledMessageSender.Shutdown();
            return this;
        }

        void IQueueMessageHandler.Handle(QueueMessage message, IMessageContext context)
        {
            if (message.Code == (int)MessageTypeCode.DomainEventStreamMessage)
            {
                var domainEventStreamMessage = _binarySerializer.Deserialize(message.Body, typeof(DomainEventStreamMessage)) as DomainEventStreamMessage;
                var domainEventStream = ConvertToDomainEventStream(domainEventStreamMessage);
                _domainEventStreamProcessor.Process(domainEventStream, new DomainEventStreamProcessContext(this, domainEventStreamMessage, message, context, domainEventStream));
            }
            else if (message.Code == (int)MessageTypeCode.EventStreamMessage)
            {
                var eventStreamMessage = _binarySerializer.Deserialize(message.Body, typeof(EventStreamMessage)) as EventStreamMessage;
                var eventStream = ConvertToEventStream(eventStreamMessage);
                _eventStreamProcessor.Process(eventStream, new EventStreamProcessContext(message, context, eventStream));
            }
            else if (message.Code == (int)MessageTypeCode.EventMessage)
            {
                var eventMessage = _binarySerializer.Deserialize(message.Body, typeof(EventMessage)) as EventMessage;
                _eventProcessor.Process(eventMessage.Event, new EventProcessContext(message, context, eventMessage.Event));
            }
            else
            {
                _logger.ErrorFormat("Invalid message code:{0}", message.Code);
                context.OnMessageHandled(message);
                return;
            }
        }

        private DomainEventStream ConvertToDomainEventStream(DomainEventStreamMessage message)
        {
            return new DomainEventStream(
                message.CommandId,
                message.AggregateRootId,
                message.AggregateRootTypeCode,
                message.Version,
                message.Timestamp,
                message.DomainEvents,
                message.Items);
        }
        private EventStream ConvertToEventStream(EventStreamMessage message)
        {
            return new EventStream(message.CommandId, message.Events, message.Items);
        }
        private IEvent ConvertToEvent(EventMessage message)
        {
            return message.Event;
        }

        class EventProcessContext : MessageProcessContext<IEvent>
        {
            public EventProcessContext(QueueMessage queueMessage, IMessageContext messageContext, IEvent evnt)
                : base(queueMessage, messageContext, evnt)
            {
            }
        }
        class EventStreamProcessContext : MessageProcessContext<IEventStream>
        {
            public EventStreamProcessContext(QueueMessage queueMessage, IMessageContext messageContext, IEventStream eventStream)
                : base(queueMessage, messageContext, eventStream)
            {
            }
        }
        class DomainEventStreamProcessContext : MessageProcessContext<IDomainEventStream>
        {
            private readonly EventConsumer _eventConsumer;
            private readonly DomainEventStreamMessage _domainEventStreamMessage;

            public DomainEventStreamProcessContext(EventConsumer eventConsumer, DomainEventStreamMessage domainEventStreamMessage, QueueMessage queueMessage, IMessageContext messageContext, IDomainEventStream domainEventStream)
                : base(queueMessage, messageContext, domainEventStream)
            {
                _eventConsumer = eventConsumer;
                _domainEventStreamMessage = domainEventStreamMessage;
            }

            public override void OnMessageProcessed(IDomainEventStream message)
            {
                base.OnMessageProcessed(message);

                if (!_eventConsumer._sendEventHandledMessage)
                {
                    return;
                }

                var topic = Constants.DomainEventHandledMessageTopic;
                var items = _domainEventStreamMessage.Items;
                if (!items.ContainsKey(topic) || string.IsNullOrEmpty(items[topic]))
                {
                    _eventConsumer._logger.ErrorFormat("{0} cannot be null or empty.", topic);
                    return;
                }
                var domainEventHandledMessageTopic = items[topic];

                _eventConsumer._domainEventHandledMessageSender.Send(new DomainEventHandledMessage
                {
                    CommandId = message.CommandId,
                    AggregateRootId = message.AggregateRootId,
                    Items = message.Items ?? new Dictionary<string, string>()
                }, domainEventHandledMessageTopic);
            }
        }
    }
}
