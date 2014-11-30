using System;
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
    public class EventPublisher : IPublisher<EventStream>, IPublisher<DomainEventStream>, IPublisher<IEvent>
    {
        private const string DefaultEventPublisherProcuderId = "EventPublisher";
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

        public void Publish(IEvent evnt)
        {
            var eventMessage = CreateEventMessage(evnt);
            var topic = _eventTopicProvider.GetTopic(evnt);
            var data = _binarySerializer.Serialize(eventMessage);
            var message = new Message(topic, (int)EQueueMessageTypeCode.EventMessage, data);
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
            var data = _binarySerializer.Serialize(eventMessage);
            var message = new Message(topic, (int)EQueueMessageTypeCode.DomainEventStreamMessage, data);
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
            var data = _binarySerializer.Serialize(eventMessage);
            var message = new Message(topic, (int)EQueueMessageTypeCode.EventStreamMessage, data);
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
            message.DomainEvents = eventStream.DomainEvents;
            message.Items = eventStream.Items;

            return message;
        }
        private EventStreamMessage CreateEventMessage(EventStream eventStream)
        {
            var message = new EventStreamMessage();

            message.CommandId = eventStream.CommandId;
            message.Events = eventStream.Events;
            message.Items = eventStream.Items;

            return message;
        }
        private EventMessage CreateEventMessage(IEvent evnt)
        {
            return new EventMessage { Event = evnt };
        }
    }
}
