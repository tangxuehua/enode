using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
        private readonly ITypeCodeProvider<IEvent> _eventTypeCodeProvider;
        private readonly IEventProcessor _eventProcessor;
        private readonly ConcurrentDictionary<string, IMessageContext> _messageContextDict;
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
            _eventTypeCodeProvider = ObjectContainer.Resolve<ITypeCodeProvider<IEvent>>();
            _eventProcessor = ObjectContainer.Resolve<IEventProcessor>();
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().FullName);
            _messageContextDict = new ConcurrentDictionary<string, IMessageContext>();
            _domainEventHandledMessageSender = domainEventHandledMessageSender ?? new DomainEventHandledMessageSender();
            _eventProcessor.Name = consumerId;
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
            var eventMessage = default(EventMessage);
            var eventStream = default(IEventStream);

            if (message.Code == (int)MessageTypeCode.DomainEventMessage)
            {
                eventMessage = _binarySerializer.Deserialize(message.Body, typeof(DomainEventMessage)) as DomainEventMessage;
                eventStream = ConvertToDomainEventStream(eventMessage as DomainEventMessage);
            }
            else if (message.Code == (int)MessageTypeCode.EventMessage)
            {
                eventMessage = _binarySerializer.Deserialize(message.Body, typeof(EventMessage)) as EventMessage;
                eventStream = ConvertToEventStream(eventMessage);
            }
            else
            {
                _logger.ErrorFormat("Invalid message code:{0}", message.Code);
                context.OnMessageHandled(message);
                return;
            }

            if (_messageContextDict.TryAdd(eventStream.CommandId, context))
            {
                _eventProcessor.Process(eventStream, new EventProcessContext(message, eventMessage, EventProcessedCallback));
            }
            else
            {
                _logger.DebugFormat("Duplicated queue message of event, topic:{0}, messageOffset:{1}", message.Topic, message.MessageOffset);
                context.OnMessageHandled(message);
            }
        }

        private void EventProcessedCallback(IEventStream eventStream, EventProcessContext eventProcessContext)
        {
            IMessageContext messageContext;
            if (_messageContextDict.TryRemove(eventStream.CommandId, out messageContext))
            {
                messageContext.OnMessageHandled(eventProcessContext.QueueMessage);
            }

            var domainEventStream = eventStream as IDomainEventStream;
            if (domainEventStream == null)
            {
                return;
            }

            if (!_sendEventHandledMessage)
            {
                return;
            }

            var items = eventProcessContext.EventMessage.Items;
            if (!items.ContainsKey("DomainEventHandledMessageTopic") || string.IsNullOrEmpty(items["DomainEventHandledMessageTopic"]))
            {
                _logger.Error("DomainEventHandledMessageTopic cannot be empty.");
                return;
            }

            var domainEventHandledMessageTopic = items["DomainEventHandledMessageTopic"];
            var processCompletedEvents = eventStream.Events.Where(x => x is IProcessCompletedEvent);
            if (processCompletedEvents.Count() > 1)
            {
                throw new Exception("One event stream cannot contains more than one IProcessCompletedEvent.");
            }

            var isProcessCompleted = processCompletedEvents.Count() == 1;
            var isProcessSuccess = false;
            var errorCode = 0;

            if (isProcessCompleted)
            {
                var processCompletedEvent = processCompletedEvents.Single() as IProcessCompletedEvent;
                isProcessSuccess = processCompletedEvent.IsSuccess;
                errorCode = processCompletedEvent.ErrorCode;
            }

            _domainEventHandledMessageSender.Send(new DomainEventHandledMessage
            {
                CommandId = domainEventStream.CommandId,
                ProcessId = domainEventStream.ProcessId,
                AggregateRootId = domainEventStream.AggregateRootId,
                IsProcessCompleted = isProcessCompleted,
                IsProcessSuccess = isProcessSuccess,
                ErrorCode = errorCode,
                Items = domainEventStream.Items ?? new Dictionary<string, string>()
            }, domainEventHandledMessageTopic);
        }
        private DomainEventStream ConvertToDomainEventStream(DomainEventMessage message)
        {
            return new DomainEventStream(
                message.CommandId,
                message.AggregateRootId,
                message.AggregateRootTypeCode,
                message.ProcessId,
                message.Version,
                message.Timestamp,
                message.DomainEvents,
                message.Items);
        }
        private EventStream ConvertToEventStream(EventMessage message)
        {
            return new EventStream(message.CommandId, message.ProcessId, message.Events, message.Items);
        }

        class EventProcessContext : IMessageProcessContext<IEventStream>
        {
            public Action<IEventStream, EventProcessContext> EventProcessedAction { get; private set; }
            public QueueMessage QueueMessage { get; private set; }
            public EventMessage EventMessage { get; private set; }

            public EventProcessContext(QueueMessage queueMessage, EventMessage eventMessage, Action<IEventStream, EventProcessContext> eventProcessedAction)
            {
                QueueMessage = queueMessage;
                EventMessage = eventMessage;
                EventProcessedAction = eventProcessedAction;
            }

            public void OnMessageProcessed(IEventStream eventStream)
            {
                EventProcessedAction(eventStream, this);
            }
        }
    }
}
