using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using ECommon.IoC;
using ECommon.Serializing;
using ECommon.Socketing;
using ENode.Eventing;
using EQueue.Clients.Consumers;
using EQueue.Protocols;

namespace ENode.EQueue
{
    public class EventConsumer : IMessageHandler
    {
        private readonly Consumer _consumer;
        private readonly IBinarySerializer _binarySerializer;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IEventTypeCodeProvider _eventTypeCodeProvider;
        private readonly IEventProcessor _eventProcessor;
        private readonly ConcurrentDictionary<Guid, IMessageContext> _messageContextDict;

        public EventConsumer() : this("DefaultEventConsumer") { }
        public EventConsumer(string groupName) : this(ConsumerSetting.Default, groupName) { }
        public EventConsumer(ConsumerSetting setting, string groupName) : this(string.Format("EventConsumer@{0}", SocketUtils.GetLocalIPV4()), setting, groupName) { }
        public EventConsumer(string id, ConsumerSetting setting, string groupName)
        {
            _binarySerializer = ObjectContainer.Resolve<IBinarySerializer>();
            _jsonSerializer = ObjectContainer.Resolve<IJsonSerializer>();
            _eventTypeCodeProvider = ObjectContainer.Resolve<IEventTypeCodeProvider>();
            _eventProcessor = ObjectContainer.Resolve<IEventProcessor>();
            _messageContextDict = new ConcurrentDictionary<Guid, IMessageContext>();
            _consumer = new Consumer(id, setting, groupName, MessageModel.Clustering, this);
        }

        public EventConsumer Start()
        {
            _consumer.Start();
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
            var eventStreamData = _binarySerializer.Deserialize(message.Body, typeof(EventStreamData)) as EventStreamData;
            var eventStream = ConvertToEventStream(eventStreamData);

            if (_messageContextDict.TryAdd(eventStream.Id, context))
            {
                _eventProcessor.Process(eventStream, new EventProcessContext(message, (processedEventStream, queueMessage) =>
                {
                    IMessageContext messageContext;
                    if (_messageContextDict.TryRemove(processedEventStream.Id, out messageContext))
                    {
                        messageContext.OnMessageHandled(queueMessage);
                    }
                }));
            }
        }

        private EventStream ConvertToEventStream(EventStreamData data)
        {
            var events = new List<IDomainEvent>();

            foreach (var typeData in data.Events)
            {
                var eventType = _eventTypeCodeProvider.GetType(typeData.TypeCode);
                var evnt = _jsonSerializer.Deserialize(typeData.Data, eventType) as IDomainEvent;
                events.Add(evnt);
            }

            return new EventStream(data.AggregateRootId, data.AggregateRootName, data.Version, data.CommandId, data.Timestamp, events);
        }

        class EventProcessContext : IEventProcessContext
        {
            public Action<EventStream, QueueMessage> EventProcessedAction { get; private set; }
            public QueueMessage QueueMessage { get; private set; }

            public EventProcessContext(QueueMessage queueMessage, Action<EventStream, QueueMessage> eventProcessedAction)
            {
                QueueMessage = queueMessage;
                EventProcessedAction = eventProcessedAction;
            }

            public void OnEventProcessed(EventStream eventStream)
            {
                EventProcessedAction(eventStream, QueueMessage);
            }
        }
    }
}
