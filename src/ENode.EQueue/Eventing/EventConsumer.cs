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
        private readonly IProcessor<DomainEventStream> _domainEventStreamProcessor;
        private readonly IProcessor<EventStream> _eventStreamProcessor;
        private readonly IProcessor<IEvent> _eventProcessor;
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
            _domainEventStreamProcessor = ObjectContainer.Resolve<IProcessor<DomainEventStream>>();
            _eventStreamProcessor = ObjectContainer.Resolve<IProcessor<EventStream>>();
            _eventProcessor = ObjectContainer.Resolve<IProcessor<IEvent>>();
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

        void IQueueMessageHandler.Handle(QueueMessage queueMessage, IMessageContext context)
        {
            if (queueMessage.Code == (int)EQueueMessageTypeCode.DomainEventStreamMessage)
            {
                var domainEventStreamMessage = _binarySerializer.Deserialize(queueMessage.Body, typeof(DomainEventStreamMessage)) as DomainEventStreamMessage;
                var domainEventStream = ConvertToDomainEventStream(domainEventStreamMessage);
                _domainEventStreamProcessor.Process(domainEventStream, new DomainEventStreamProcessContext(this, domainEventStreamMessage, queueMessage, context, domainEventStream));
            }
            else if (queueMessage.Code == (int)EQueueMessageTypeCode.EventStreamMessage)
            {
                var eventStreamMessage = _binarySerializer.Deserialize(queueMessage.Body, typeof(EventStreamMessage)) as EventStreamMessage;
                var eventStream = ConvertToEventStream(eventStreamMessage);
                _eventStreamProcessor.Process(eventStream, new EventStreamProcessContext(queueMessage, context, eventStream));
            }
            else if (queueMessage.Code == (int)EQueueMessageTypeCode.EventMessage)
            {
                var eventMessage = _binarySerializer.Deserialize(queueMessage.Body, typeof(EventMessage)) as EventMessage;
                _eventProcessor.Process(eventMessage.Event, new EventProcessContext(queueMessage, context, eventMessage.Event));
            }
            else
            {
                _logger.ErrorFormat("Invalid message code:{0}", queueMessage.Code);
                context.OnMessageHandled(queueMessage);
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

        class EventProcessContext : EQueueProcessContext<IEvent>
        {
            public EventProcessContext(QueueMessage queueMessage, IMessageContext messageContext, IEvent evnt)
                : base(queueMessage, messageContext, evnt)
            {
            }
        }
        class EventStreamProcessContext : EQueueProcessContext<EventStream>
        {
            public EventStreamProcessContext(QueueMessage queueMessage, IMessageContext messageContext, EventStream eventStream)
                : base(queueMessage, messageContext, eventStream)
            {
            }
        }
        class DomainEventStreamProcessContext : EQueueProcessContext<DomainEventStream>
        {
            private readonly EventConsumer _eventConsumer;
            private readonly DomainEventStreamMessage _domainEventStreamMessage;

            public DomainEventStreamProcessContext(EventConsumer eventConsumer, DomainEventStreamMessage domainEventStreamMessage, QueueMessage queueMessage, IMessageContext messageContext, DomainEventStream domainEventStream)
                : base(queueMessage, messageContext, domainEventStream)
            {
                _eventConsumer = eventConsumer;
                _domainEventStreamMessage = domainEventStreamMessage;
            }

            public override void OnProcessed(DomainEventStream message)
            {
                base.OnProcessed(message);

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
