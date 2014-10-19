using System;
using System.Collections.Generic;
using System.Linq;
using ECommon.Components;
using ECommon.Logging;
using ECommon.Serializing;
using ENode.Eventing;
using ENode.Infrastructure;
using EQueue.Clients.Producers;
using EQueue.Protocols;

namespace ENode.EQueue
{
    public class EventPublisher : IMessagePublisher<EventStream>, IMessagePublisher<DomainEventStream>
    {
        private const string DefaultEventPublisherProcuderId = "sys_epp";
        private readonly ILogger _logger;
        private readonly IBinarySerializer _binarySerializer;
        private readonly ITopicProvider<IEvent> _eventTopicProvider;
        private readonly Producer _producer;

        public Producer Producer { get { return _producer; } }

        public EventPublisher(string id = null, ProducerSetting setting = null)
        {
            _producer = new Producer(id ?? DefaultEventPublisherProcuderId, setting ?? new ProducerSetting());
            _binarySerializer = ObjectContainer.Resolve<IBinarySerializer>();
            _eventTopicProvider = ObjectContainer.Resolve<ITopicProvider<IEvent>>();
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().FullName);
        }

        public EventPublisher Start()
        {
            _producer.Start();
            return this;
        }
        public EventPublisher Shutdown()
        {
            _producer.Shutdown();
            return this;
        }

        public void Publish(DomainEventStream eventStream)
        {
            var eventMessage = CreateEventMessage(eventStream);
            var topic = _eventTopicProvider.GetTopic(eventStream.Events.First());
            var data = _binarySerializer.Serialize(eventMessage);
            var message = new Message(topic, (int)MessageTypeCode.DomainEventMessage, data);
            var result = _producer.Send(message, eventStream.AggregateRootId);
            if (result.SendStatus != SendStatus.Success)
            {
                throw new Exception(string.Format("Publish domain event failed, eventStream:[{0}]", eventStream));
            }
        }
        public void Publish(EventStream eventStream)
        {
            var eventMessage = CreateEventMessage(eventStream);
            var topic = _eventTopicProvider.GetTopic(eventStream.Events.First());
            var data = _binarySerializer.Serialize(eventMessage);
            var message = new Message(topic, (int)MessageTypeCode.EventMessage, data);
            var result = _producer.Send(message, eventMessage.CommandId);
            if (result.SendStatus != SendStatus.Success)
            {
                throw new Exception(string.Format("Publish event failed, eventStream:[{0}]", eventStream));
            }
        }

        private DomainEventMessage CreateEventMessage(DomainEventStream eventStream)
        {
            var message = new DomainEventMessage();

            message.CommandId = eventStream.CommandId;
            message.AggregateRootId = eventStream.AggregateRootId;
            message.AggregateRootTypeCode = eventStream.AggregateRootTypeCode;
            message.Timestamp = eventStream.Timestamp;
            message.ProcessId = eventStream.ProcessId;
            message.Version = eventStream.Version;
            message.DomainEvents = eventStream.DomainEvents;
            message.Items = eventStream.Items;

            return message;
        }
        private EventMessage CreateEventMessage(EventStream eventStream)
        {
            var message = new EventMessage();

            message.CommandId = eventStream.CommandId;
            message.ProcessId = eventStream.ProcessId;
            message.Events = eventStream.Events;
            message.Items = eventStream.Items;

            return message;
        }
    }
}
