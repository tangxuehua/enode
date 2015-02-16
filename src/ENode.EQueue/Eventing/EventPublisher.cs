using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private readonly IOHelper _ioHelper;

        public Producer Producer { get { return _producer; } }

        public EventPublisher(string id = null, ProducerSetting setting = null)
        {
            _producer = new Producer(id ?? DefaultEventPublisherProcuderId, setting ?? new ProducerSetting());
            _jsonSerializer = ObjectContainer.Resolve<IJsonSerializer>();
            _eventTopicProvider = ObjectContainer.Resolve<ITopicProvider<IEvent>>();
            _eventTypeCodeProvider = ObjectContainer.Resolve<ITypeCodeProvider<IEvent>>();
            _eventSerializer = ObjectContainer.Resolve<IEventSerializer>();
            _ioHelper = ObjectContainer.Resolve<IOHelper>();
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
            var message = CreateEQueueMessage(evnt);
            _ioHelper.TryIOAction(() =>
            {
                var routingKey = evnt is IDomainEvent ? ((IDomainEvent)evnt).AggregateRootId : evnt.Id;
                var result = _producer.Send(message, routingKey);
                if (result.SendStatus != SendStatus.Success)
                {
                    throw new IOException(result.ErrorMessage);
                }
            }, "Send event message to broker");
        }
        public async Task<PublishResult<IEvent>> PublishAsync(IEvent evnt)
        {
            var message = CreateEQueueMessage(evnt);
            return await _ioHelper.TryIOFuncAsync(() =>
            {
                var routingKey = evnt is IDomainEvent ? ((IDomainEvent)evnt).AggregateRootId : evnt.Id;
                return _producer.SendAsync(message, routingKey).ContinueWith<PublishResult<IEvent>>(t =>
                {
                    if (t.Result.SendStatus != SendStatus.Success)
                    {
                        return new PublishResult<IEvent>(PublishStatus.IOException, t.Result.ErrorMessage, evnt);
                    }
                    return new PublishResult<IEvent>(PublishStatus.Success, null, evnt);
                });
            }, "Send event message to broker");
        }
        public void Publish(DomainEventStream eventStream)
        {
            var message = CreateEQueueMessage(eventStream);
            _ioHelper.TryIOAction(() =>
            {
                var result = _producer.Send(message, eventStream.AggregateRootId);
                if (result.SendStatus != SendStatus.Success)
                {
                    throw new IOException(result.ErrorMessage);
                }
            }, "Send domain event stream message to broker");
        }
        public async Task<PublishResult<DomainEventStream>> PublishAsync(DomainEventStream eventStream)
        {
            var message = CreateEQueueMessage(eventStream);
            return await _ioHelper.TryIOFuncAsync(() =>
            {
                return _producer.SendAsync(message, eventStream.AggregateRootId).ContinueWith<PublishResult<DomainEventStream>>(t =>
                {
                    if (t.Result.SendStatus != SendStatus.Success)
                    {
                        return new PublishResult<DomainEventStream>(PublishStatus.IOException, t.Result.ErrorMessage, eventStream);
                    }
                    return new PublishResult<DomainEventStream>(PublishStatus.Success, null, eventStream);
                });
            }, "Send domain event stream message to broker");
        }
        public void Publish(EventStream eventStream)
        {
            var message = CreateEQueueMessage(eventStream);
            _ioHelper.TryIOAction(() =>
            {
                var result = _producer.Send(message, eventStream.CommandId);
                if (result.SendStatus != SendStatus.Success)
                {
                    throw new IOException(result.ErrorMessage);
                }
            }, "Send event stream message to broker");
        }
        public async Task<PublishResult<EventStream>> PublishAsync(EventStream eventStream)
        {
            var message = CreateEQueueMessage(eventStream);
            return await _ioHelper.TryIOFuncAsync(() =>
            {
                return _producer.SendAsync(message, eventStream.CommandId).ContinueWith<PublishResult<EventStream>>(t =>
                {
                    if (t.Result.SendStatus != SendStatus.Success)
                    {
                        return new PublishResult<EventStream>(PublishStatus.IOException, t.Result.ErrorMessage, eventStream);
                    }
                    return new PublishResult<EventStream>(PublishStatus.Success, null, eventStream);
                });
            }, "Send event stream message to broker");
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
