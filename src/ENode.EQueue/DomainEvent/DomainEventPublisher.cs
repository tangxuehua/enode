using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECommon.Components;
using ECommon.IO;
using ECommon.Serializing;
using ECommon.Utilities;
using ENode.Eventing;
using ENode.Messaging;
using EQueue.Clients.Producers;
using EQueueMessage = EQueue.Protocols.Message;

namespace ENode.EQueue
{
    public class DomainEventPublisher : IMessagePublisher<DomainEventStreamMessage>
    {
        private IJsonSerializer _jsonSerializer;
        private ITopicProvider<IDomainEvent> _eventTopicProvider;
        private IEventSerializer _eventSerializer;
        private SendQueueMessageService _sendMessageService;

        public Producer Producer { get; private set; }

        public DomainEventPublisher InitializeENode()
        {
            _jsonSerializer = ObjectContainer.Resolve<IJsonSerializer>();
            _eventTopicProvider = ObjectContainer.Resolve<ITopicProvider<IDomainEvent>>();
            _eventSerializer = ObjectContainer.Resolve<IEventSerializer>();
            _sendMessageService = new SendQueueMessageService();
            return this;
        }
        public DomainEventPublisher InitializeEQueue(ProducerSetting setting = null)
        {
            InitializeENode();
            Producer = new Producer(setting, "DomainEventPublisher");
            return this;
        }

        public DomainEventPublisher Start()
        {
            Producer.Start();
            return this;
        }
        public DomainEventPublisher Shutdown()
        {
            Producer.Shutdown();
            return this;
        }
        public Task PublishAsync(DomainEventStreamMessage eventStream)
        {
            Ensure.NotNull(eventStream.AggregateRootId, "aggregateRootId");
            var eventMessage = CreateEventMessage(eventStream);
            var topic = _eventTopicProvider.GetTopic(eventStream.Events.First());
            var data = _jsonSerializer.Serialize(eventMessage);
            var equeueMessage = new EQueueMessage(topic, (int)EQueueMessageTypeCode.DomainEventStreamMessage, Encoding.UTF8.GetBytes(data));

            return _sendMessageService.SendMessageAsync(Producer, "events", string.Join(",", eventStream.Events.Select(x => x.GetType().Name)), equeueMessage, data, eventStream.AggregateRootId, eventStream.Id, eventStream.Items);
        }

        private EventStreamMessage CreateEventMessage(DomainEventStreamMessage eventStream)
        {
            var message = new EventStreamMessage
            {
                Id = eventStream.Id,
                CommandId = eventStream.CommandId,
                AggregateRootTypeName = eventStream.AggregateRootTypeName,
                AggregateRootId = eventStream.AggregateRootId,
                Timestamp = eventStream.Timestamp,
                Version = eventStream.Version,
                Events = _eventSerializer.Serialize(eventStream.Events),
                Items = eventStream.Items
            };

            return message;
        }
    }
}
