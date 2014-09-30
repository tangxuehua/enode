using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ECommon.Components;
using ECommon.Logging;
using ECommon.Serializing;
using ENode.Eventing;
using EQueue.Clients.Consumers;
using EQueue.Protocols;

namespace ENode.EQueue
{
    public class EventConsumer : IMessageHandler
    {
        private const string DefaultEventConsumerId = "EventConsumer";
        private const string DefaultEventConsumerGroup = "EventConsumerGroup";
        private readonly Consumer _consumer;
        private readonly DomainEventHandledMessageSender _domainEventHandledMessageSender;
        private readonly IBinarySerializer _binarySerializer;
        private readonly IEventTypeCodeProvider _eventTypeCodeProvider;
        private readonly IEventProcessor _eventProcessor;
        private readonly IDomainEventProcessor _domainEventProcessor;
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
            _eventTypeCodeProvider = ObjectContainer.Resolve<IEventTypeCodeProvider>();
            _eventProcessor = ObjectContainer.Resolve<IEventProcessor>();
            _domainEventProcessor = ObjectContainer.Resolve<IDomainEventProcessor>();
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().FullName);
            _messageContextDict = new ConcurrentDictionary<string, IMessageContext>();
            _domainEventHandledMessageSender = domainEventHandledMessageSender ?? new DomainEventHandledMessageSender();
            _domainEventProcessor.Name = consumerId;
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

        void IMessageHandler.Handle(QueueMessage message, IMessageContext context)
        {
            if (message.Code == (int)MessageTypeCode.DomainEventMessage)
            {
                var eventMessage = _binarySerializer.Deserialize(message.Body, typeof(DomainEventMessage)) as DomainEventMessage;
                var eventStream = ConvertToEventStream(eventMessage);

                if (_messageContextDict.TryAdd(eventStream.CommandId, context))
                {
                    _domainEventProcessor.Process(eventStream, new DomainEventProcessContext(message, eventMessage, DomainEventProcessedCallback));
                }
                else
                {
                    _logger.DebugFormat("Duplicated queue message of domain event, topic:{0}, messageOffset:{1}", message.Topic, message.MessageOffset);
                    context.OnMessageHandled(message);
                }
            }
            else if (message.Code == (int)MessageTypeCode.EventMessage)
            {
                var eventMessage = _binarySerializer.Deserialize(message.Body, typeof(EventMessage)) as EventMessage;
                var eventStream = ConvertToEventStream(eventMessage);
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
            else
            {
                _logger.ErrorFormat("Invalid message code:{0}", message.Code);
                context.OnMessageHandled(message);
            }
        }

        private void EventProcessedCallback(EventStream eventStream, EventProcessContext eventProcessContext)
        {
            IMessageContext messageContext;
            if (_messageContextDict.TryRemove(eventStream.CommandId, out messageContext))
            {
                messageContext.OnMessageHandled(eventProcessContext.QueueMessage);
            }
        }
        private void DomainEventProcessedCallback(DomainEventStream eventStream, DomainEventProcessContext eventProcessContext)
        {
            IMessageContext messageContext;
            if (_messageContextDict.TryRemove(eventStream.CommandId, out messageContext))
            {
                messageContext.OnMessageHandled(eventProcessContext.QueueMessage);
            }

            if (!_sendEventHandledMessage)
            {
                return;
            }

            var contextItems = eventProcessContext.EventMessage.ContextItems;
            if (!ValidateContextItems(contextItems))
            {
                return;
            }

            var domainEventHandledMessageTopic = contextItems["DomainEventHandledMessageTopic"];
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
                CommandId = eventStream.CommandId,
                ProcessId = eventStream.ProcessId,
                AggregateRootId = eventStream.AggregateRootId,
                IsProcessCompleted = isProcessCompleted,
                IsProcessSuccess = isProcessSuccess,
                ErrorCode = errorCode,
                Items = eventStream.Items ?? new Dictionary<string, string>()
            }, domainEventHandledMessageTopic);
        }
        private bool ValidateContextItems(IDictionary<string, string> contextItems)
        {
            if (!contextItems.ContainsKey("DomainEventHandledMessageTopic"))
            {
                _logger.Error("Key 'DomainEventHandledMessageTopic' missing in event message context items dict.");
                return false;
            }
            else if (string.IsNullOrEmpty(contextItems["DomainEventHandledMessageTopic"]))
            {
                _logger.Error("DomainEventHandledMessageTopic cannot be empty.");
                return false;
            }
            return true;
        }
        private DomainEventStream ConvertToEventStream(DomainEventMessage message)
        {
            return new DomainEventStream(
                message.CommandId,
                message.AggregateRootId,
                message.AggregateRootTypeCode,
                message.ProcessId,
                message.Version,
                message.Timestamp,
                message.Events,
                message.Items);
        }
        private EventStream ConvertToEventStream(EventMessage message)
        {
            return new EventStream(message.CommandId, message.ProcessId, message.Events, message.ContextItems);
        }

        class EventProcessContext : IEventProcessContext
        {
            public Action<EventStream, EventProcessContext> EventProcessedAction { get; private set; }
            public QueueMessage QueueMessage { get; private set; }
            public EventMessage EventMessage { get; private set; }

            public EventProcessContext(QueueMessage queueMessage, EventMessage eventMessage, Action<EventStream, EventProcessContext> eventProcessedAction)
            {
                QueueMessage = queueMessage;
                EventMessage = eventMessage;
                EventProcessedAction = eventProcessedAction;
            }

            public void OnEventProcessed(EventStream eventStream)
            {
                EventProcessedAction(eventStream, this);
            }
        }
        class DomainEventProcessContext : IDomainEventProcessContext
        {
            public Action<DomainEventStream, DomainEventProcessContext> EventProcessedAction { get; private set; }
            public QueueMessage QueueMessage { get; private set; }
            public DomainEventMessage EventMessage { get; private set; }

            public DomainEventProcessContext(QueueMessage queueMessage, DomainEventMessage eventMessage, Action<DomainEventStream, DomainEventProcessContext> eventProcessedAction)
            {
                QueueMessage = queueMessage;
                EventMessage = eventMessage;
                EventProcessedAction = eventProcessedAction;
            }

            public void OnEventProcessed(DomainEventStream eventStream)
            {
                EventProcessedAction(eventStream, this);
            }
        }
    }
}
