using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECommon.Components;
using ECommon.IO;
using ECommon.Serializing;
using ECommon.Utilities;
using ENode.Eventing;
using ENode.Infrastructure;
using EQueue.Clients.Producers;
using EQueueMessage = EQueue.Protocols.Message;

namespace ENode.EQueue
{
    public class DomainEventPublisher : IMessagePublisher<DomainEventStreamMessage>
    {
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ITopicProvider<IDomainEvent> _eventTopicProvider;
        private readonly ITypeCodeProvider _eventTypeCodeProvider;
        private readonly IEventSerializer _eventSerializer;
        private readonly Producer _producer;
        private readonly SendQueueMessageService _sendMessageService;

        public Producer Producer { get { return _producer; } }

        public DomainEventPublisher(ProducerSetting setting = null)
        {
            _producer = new Producer(setting);
            _jsonSerializer = ObjectContainer.Resolve<IJsonSerializer>();
            _eventTopicProvider = ObjectContainer.Resolve<ITopicProvider<IDomainEvent>>();
            _eventTypeCodeProvider = ObjectContainer.Resolve<ITypeCodeProvider>();
            _eventSerializer = ObjectContainer.Resolve<IEventSerializer>();
            _sendMessageService = new SendQueueMessageService();
        }

        public DomainEventPublisher Start()
        {
            _producer.Start();
            return this;
        }
        public DomainEventPublisher Shutdown()
        {
            _producer.Shutdown();
            return this;
        }
        public Task<AsyncTaskResult> PublishAsync(DomainEventStreamMessage eventStream)
        {
            var message = CreateEQueueMessage(eventStream);
            return _sendMessageService.SendMessageAsync(_producer, message, eventStream.GetRoutingKey() ?? eventStream.AggregateRootId);
        }

        private EQueueMessage CreateEQueueMessage(DomainEventStreamMessage eventStream)
        {
            Ensure.NotNull(eventStream.AggregateRootId, "aggregateRootId");
            var eventMessage = CreateEventMessage(eventStream);
            var topic = _eventTopicProvider.GetTopic(eventStream.Events.First());
            var data = _jsonSerializer.Serialize(eventMessage);
            return new EQueueMessage(topic, (int)EQueueMessageTypeCode.DomainEventStreamMessage, Encoding.UTF8.GetBytes(data));
        }
        private EventStreamMessage CreateEventMessage(DomainEventStreamMessage eventStream)
        {
            var message = new EventStreamMessage();

            message.CommandId = eventStream.CommandId;
            message.AggregateRootTypeCode = eventStream.AggregateRootTypeCode;
            message.AggregateRootId = eventStream.AggregateRootId;
            message.Timestamp = eventStream.Timestamp;
            message.Version = eventStream.Version;
            message.Events = _eventSerializer.Serialize(eventStream.Events);
            message.Items = eventStream.Items;

            return message;
        }
    }
}
