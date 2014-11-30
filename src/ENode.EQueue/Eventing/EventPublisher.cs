using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ECommon.Components;
using ECommon.Logging;
using ECommon.Serializing;
using ENode.Eventing;
using ENode.Infrastructure;
using EQueue.Clients.Producers;
using EQueue.Protocols;

namespace ENode.EQueue
{
    public class EventPublisher : IPublisher<EventStream>, IPublisher<DomainEventStream>, IPublisher<IEvent>
    {
        private const string DefaultEventPublisherProcuderId = "EventPublisher";
        private readonly ILogger _logger;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ITopicProvider<IEvent> _eventTopicProvider;
        private readonly ITypeCodeProvider<IEvent> _eventTypeCodeProvider;
        private readonly IEventSerializer _eventSerializer;
        private readonly Producer _producer;

        public Producer Producer { get { return _producer; } }

        public EventPublisher(string id = null, ProducerSetting setting = null)
        {
            _producer = new Producer(id ?? DefaultEventPublisherProcuderId, setting ?? new ProducerSetting());
            _jsonSerializer = ObjectContainer.Resolve<IJsonSerializer>();
            _eventTopicProvider = ObjectContainer.Resolve<ITopicProvider<IEvent>>();
            _eventTypeCodeProvider = ObjectContainer.Resolve<ITypeCodeProvider<IEvent>>();
            _eventSerializer = ObjectContainer.Resolve<IEventSerializer>();
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

        public void Publish(IEvent evnt)
        {
            var eventTypeCode = _eventTypeCodeProvider.GetTypeCode(evnt.GetType());
            var eventData = _jsonSerializer.Serialize(evnt);
            var topic = _eventTopicProvider.GetTopic(evnt);
            var data = _jsonSerializer.Serialize(new EventMessage { EventTypeCode = eventTypeCode, EventData = eventData });
            var message = new Message(topic, (int)EQueueMessageTypeCode.EventMessage, Encoding.UTF8.GetBytes(data));
            var result = _producer.Send(message, evnt is IDomainEvent ? ((IDomainEvent)evnt).AggregateRootId : evnt.Id);
            if (result.SendStatus != SendStatus.Success)
            {
                var domainEvent = evnt as IDomainEvent;
                if (domainEvent != null)
                {
                    throw new Exception(string.Format("Publish domain event failed, event:[id:{0},type:{1},aggregateRootId:{2},version:{3}]", domainEvent.Id, domainEvent.GetType().FullName, domainEvent.AggregateRootId, domainEvent.Version));
                }
                else
                {
                    throw new Exception(string.Format("Publish event failed, event:[id:{0},type:{1}]", evnt.Id, evnt.GetType().FullName));
                }
            }
        }
        public void Publish(DomainEventStream eventStream)
        {
            var eventMessage = CreateEventMessage(eventStream);
            var topic = _eventTopicProvider.GetTopic(eventStream.Events.First());
            var data = _jsonSerializer.Serialize(eventMessage);
            var message = new Message(topic, (int)EQueueMessageTypeCode.DomainEventStreamMessage, Encoding.UTF8.GetBytes(data));
            var result = _producer.Send(message, eventStream.AggregateRootId);
            if (result.SendStatus != SendStatus.Success)
            {
                throw new Exception(string.Format("Publish domain event stream failed, eventStream:[{0}]", eventStream));
            }
        }
        public void Publish(EventStream eventStream)
        {
            var eventMessage = CreateEventMessage(eventStream);
            var topic = _eventTopicProvider.GetTopic(eventStream.Events.First());
            var data = _jsonSerializer.Serialize(eventMessage);
            var message = new Message(topic, (int)EQueueMessageTypeCode.EventStreamMessage, Encoding.UTF8.GetBytes(data));
            var result = _producer.Send(message, eventMessage.CommandId);
            if (result.SendStatus != SendStatus.Success)
            {
                throw new Exception(string.Format("Publish event stream failed, eventStream:[{0}]", eventStream));
            }
        }

        private DomainEventStreamMessage CreateEventMessage(DomainEventStream eventStream)
        {
            var message = new DomainEventStreamMessage();

            message.CommandId = eventStream.CommandId;
            message.AggregateRootId = eventStream.AggregateRootId;
            message.AggregateRootTypeCode = eventStream.AggregateRootTypeCode;
            message.Timestamp = eventStream.Timestamp;
            message.Version = eventStream.Version;
            message.Events = _eventSerializer.Serialize(eventStream.DomainEvents);
            message.Items = eventStream.Items;

            return message;
        }
        private EventStreamMessage CreateEventMessage(EventStream eventStream)
        {
            var message = new EventStreamMessage();

            message.CommandId = eventStream.CommandId;
            message.Events = _eventSerializer.Serialize(eventStream.Events);
            message.Items = eventStream.Items;

            return message;
        }
        private EventMessage CreateEventMessage(IEvent evnt)
        {
            return new EventMessage
            {
                EventData = _jsonSerializer.Serialize(evnt),
                EventTypeCode = _eventTypeCodeProvider.GetTypeCode(evnt.GetType())
            };
        }
    }
}
