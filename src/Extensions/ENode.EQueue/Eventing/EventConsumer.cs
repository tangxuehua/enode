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

        void IMessageHandler.Handle(QueueMessage message, IMessageContext context)
        {
            var eventMessage = _binarySerializer.Deserialize(message.Body, typeof(EventMessage)) as EventMessage;
            var eventStream = ConvertToEventStream(eventMessage);

            if (_messageContextDict.TryAdd(eventStream.CommandId, context))
            {
                _eventProcessor.Process(eventStream, new EventProcessContext(message, eventMessage, EventProcessedCallback));
            }
        }

        private void EventProcessedCallback(EventStream eventStream, EventProcessContext eventProcessContext)
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
            var currentCommandId = contextItems["CurrentCommandId"];
            var currentProcessId = contextItems["CurrentProcessId"];
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
                CommandId = currentCommandId,
                ProcessId = currentProcessId,
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
            else if (!contextItems.ContainsKey("CurrentCommandId"))
            {
                _logger.Error("Key 'CurrentCommandId' missing in event message context items dict.");
                return false;
            }
            else if (!contextItems.ContainsKey("CurrentProcessId"))
            {
                _logger.Error("Key 'CurrentProcessId' missing in event message context items dict.");
                return false;
            }
            else if (string.IsNullOrEmpty(contextItems["DomainEventHandledMessageTopic"]))
            {
                _logger.Error("DomainEventHandledMessageTopic cannot be empty.");
                return false;
            }
            else if (string.IsNullOrEmpty(contextItems["CurrentCommandId"]))
            {
                _logger.Error("CurrentCommandId cannot be empty.");
                return false;
            }
            return true;
        }
        private EventStream ConvertToEventStream(EventMessage data)
        {
            return new EventStream(data.CommandId, data.AggregateRootId, data.AggregateRootTypeCode, data.ProcessId, data.Version, data.Timestamp, data.Events, data.Items);
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
    }
}
