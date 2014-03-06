using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ECommon.IoC;
using ECommon.Serializing;
using ECommon.Socketing;
using ECommon.Utilities;
using ENode.Eventing;
using EQueue.Clients.Consumers;
using EQueue.Protocols;

namespace ENode.EQueue
{
    public class EventConsumer : IMessageHandler
    {
        private readonly Consumer _consumer;
        private readonly DomainEventHandledMessageSender _domainEventHandledMessageSender;
        private readonly IBinarySerializer _binarySerializer;
        private readonly IEventTypeCodeProvider _eventTypeCodeProvider;
        private readonly IEventProcessor _eventProcessor;
        private readonly ConcurrentDictionary<string, IMessageContext> _messageContextDict;
        private readonly static ConsumerSetting _consumerSetting = new ConsumerSetting
        {
            MessageHandleMode = MessageHandleMode.Sequential
        };

        public Consumer Consumer { get { return _consumer; } }

        public EventConsumer(DomainEventHandledMessageSender domainEventHandledMessageSender)
            : this(_consumerSetting, domainEventHandledMessageSender)
        {
        }
        public EventConsumer(string groupName, DomainEventHandledMessageSender domainEventHandledMessageSender)
            : this(_consumerSetting, groupName, domainEventHandledMessageSender)
        {
        }
        public EventConsumer(ConsumerSetting setting, DomainEventHandledMessageSender domainEventHandledMessageSender)
            : this(setting, null, domainEventHandledMessageSender)
        {
        }
        public EventConsumer(ConsumerSetting setting, string groupName, DomainEventHandledMessageSender domainEventHandledMessageSender)
            : this(setting, null, groupName, domainEventHandledMessageSender)
        {
        }
        public EventConsumer(ConsumerSetting setting, string name, string groupName, DomainEventHandledMessageSender domainEventHandledMessageSender)
            : this(string.Format("{0}@{1}@{2}", SocketUtils.GetLocalIPV4(), string.IsNullOrEmpty(name) ? typeof(EventConsumer).Name : name, ObjectId.GenerateNewId()), setting, groupName, domainEventHandledMessageSender)
        {
        }
        public EventConsumer(string id, ConsumerSetting setting, string groupName, DomainEventHandledMessageSender domainEventHandledMessageSender)
        {
            _consumer = new Consumer(id, setting, string.IsNullOrEmpty(groupName) ? typeof(EventConsumer).Name + "Group" : groupName);
            _binarySerializer = ObjectContainer.Resolve<IBinarySerializer>();
            _eventTypeCodeProvider = ObjectContainer.Resolve<IEventTypeCodeProvider>();
            _eventProcessor = ObjectContainer.Resolve<IEventProcessor>();
            _messageContextDict = new ConcurrentDictionary<string, IMessageContext>();
            _domainEventHandledMessageSender = domainEventHandledMessageSender;
        }

        public EventConsumer Start()
        {
            _consumer.Start(this);
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
            return this;
        }

        void IMessageHandler.Handle(QueueMessage message, IMessageContext context)
        {
            var eventMessage = _binarySerializer.Deserialize(message.Body, typeof(EventMessage)) as EventMessage;
            var eventStream = ConvertToEventStream(eventMessage);

            if (_messageContextDict.TryAdd(eventStream.CommitId, context))
            {
                _eventProcessor.Process(eventStream, new EventProcessContext(message, eventMessage, EventHandledCallback));
            }
        }

        private void EventHandledCallback(EventStream eventStream, EventProcessContext eventProcessContext)
        {
            IMessageContext messageContext;
            if (_messageContextDict.TryRemove(eventStream.CommitId, out messageContext))
            {
                messageContext.OnMessageHandled(eventProcessContext.QueueMessage);
            }

            if (eventProcessContext.EventMessage.ContextItems != null && eventProcessContext.EventMessage.ContextItems.ContainsKey("DomainEventHandledMessageTopic"))
            {
                var domainEventHandledMessageTopic = eventProcessContext.EventMessage.ContextItems["DomainEventHandledMessageTopic"] as string;
                var processCompletedEvent = eventStream.Events.FirstOrDefault(x => x is IProcessCompletedEvent) as IProcessCompletedEvent;
                var processId = default(string);
                var isProcessCompletedEvent = false;

                if (processCompletedEvent != null)
                {
                    isProcessCompletedEvent = true;
                    processId = processCompletedEvent.ProcessId;
                }

                _domainEventHandledMessageSender.Send(new DomainEventHandledMessage
                {
                    CommandId = eventStream.CommitId,
                    AggregateRootId = eventStream.AggregateRootId,
                    IsProcessCompletedEvent = isProcessCompletedEvent,
                    ProcessId = processId
                }, domainEventHandledMessageTopic);
            }
        }

        private EventStream ConvertToEventStream(EventMessage data)
        {
            var events = new List<IDomainEvent>();

            foreach (var typeData in data.Events)
            {
                var eventType = _eventTypeCodeProvider.GetType(typeData.EventTypeCode);
                var evnt = _binarySerializer.Deserialize(typeData.EventData, eventType) as IDomainEvent;
                events.Add(evnt);
            }

            return new EventStream(data.CommitId, data.AggregateRootId, data.AggregateRootTypeCode, data.Version, data.Timestamp, events);
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
