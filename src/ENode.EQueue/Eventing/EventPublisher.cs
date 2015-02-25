using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECommon.Components;
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
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ITopicProvider<IEvent> _eventTopicProvider;
        private readonly ITypeCodeProvider<IEvent> _eventTypeCodeProvider;
        private readonly IEventSerializer _eventSerializer;
        private readonly Producer _producer;
        private readonly PublisherHelper _publisherHelper;

        public Producer Producer { get { return _producer; } }

        public EventPublisher(string id = null, ProducerSetting setting = null)
        {
            _producer = new Producer(id ?? DefaultEventPublisherProcuderId, setting ?? new ProducerSetting());
            _jsonSerializer = ObjectContainer.Resolve<IJsonSerializer>();
            _eventTopicProvider = ObjectContainer.Resolve<ITopicProvider<IEvent>>();
            _eventTypeCodeProvider = ObjectContainer.Resolve<ITypeCodeProvider<IEvent>>();
            _eventSerializer = ObjectContainer.Resolve<IEventSerializer>();
            _publisherHelper = new PublisherHelper();
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
            var message = CreateEQueueMessage(evnt);
            var routingKey = evnt is IDomainEvent ? ((IDomainEvent)evnt).AggregateRootId : evnt.Id;
            _publisherHelper.PublishQueueMessage(_producer, message, routingKey, "Send event message to broker");
        }
        public void Publish(DomainEventStream eventStream)
        {
            var message = CreateEQueueMessage(eventStream);
            _publisherHelper.PublishQueueMessage(_producer, message, eventStream.AggregateRootId, "Send domain event stream message to broker");
        }
        public void Publish(EventStream eventStream)
        {
            var message = CreateEQueueMessage(eventStream);
            _publisherHelper.PublishQueueMessage(_producer, message, eventStream.CommandId, "Send event stream message to broker");
        }

        public Task<AsyncOperationResult> PublishAsync(IEvent evnt)
        {
            var message = CreateEQueueMessage(evnt);
            var routingKey = evnt is IDomainEvent ? ((IDomainEvent)evnt).AggregateRootId : evnt.Id;
            return _publisherHelper.PublishQueueMessageAsync(_producer, message, routingKey, "Send event message to broker");
        }
        public Task<AsyncOperationResult> PublishAsync(DomainEventStream eventStream)
        {
            var message = CreateEQueueMessage(eventStream);
            return _publisherHelper.PublishQueueMessageAsync(_producer, message, eventStream.AggregateRootId, "Send domain event stream message to broker");
        }
        public Task<AsyncOperationResult> PublishAsync(EventStream eventStream)
        {
            var message = CreateEQueueMessage(eventStream);
            return _publisherHelper.PublishQueueMessageAsync(_producer, message, eventStream.CommandId, "Send event stream message to broker");
        }

        private Message CreateEQueueMessage(IEvent evnt)
        {
            var eventTypeCode = _eventTypeCodeProvider.GetTypeCode(evnt.GetType());
            var eventData = _jsonSerializer.Serialize(evnt);
            var topic = _eventTopicProvider.GetTopic(evnt);
            var data = _jsonSerializer.Serialize(new EventMessage { EventTypeCode = eventTypeCode, EventData = eventData });
            return new Message(topic, (int)EQueueMessageTypeCode.EventMessage, Encoding.UTF8.GetBytes(data));
        }
        private Message CreateEQueueMessage(DomainEventStream eventStream)
        {
            var eventMessage = CreateEventMessage(eventStream);
            var topic = _eventTopicProvider.GetTopic(eventStream.Events.First());
            var data = _jsonSerializer.Serialize(eventMessage);
            return new Message(topic, (int)EQueueMessageTypeCode.DomainEventStreamMessage, Encoding.UTF8.GetBytes(data));
        }
        private Message CreateEQueueMessage(EventStream eventStream)
        {
            var eventMessage = CreateEventMessage(eventStream);
            var topic = _eventTopicProvider.GetTopic(eventStream.Events.First());
            var data = _jsonSerializer.Serialize(eventMessage);
            return new Message(topic, (int)EQueueMessageTypeCode.EventStreamMessage, Encoding.UTF8.GetBytes(data));
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
