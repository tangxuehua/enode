using System.Collections.Generic;
using ECommon.Components;
using ECommon.Logging;
using ECommon.Serializing;
using ENode.Eventing;
using ENode.Infrastructure;
using EQueue.Clients.Producers;
using EQueue.Protocols;

namespace ENode.EQueue
{
    public class EventPublisher : IEventPublisher
    {
        private const string DefaultEventPublisherProcuderId = "sys_epp";
        private readonly ILogger _logger;
        private readonly IBinarySerializer _binarySerializer;
        private readonly IEventTopicProvider _eventTopicProvider;
        private readonly IEventTypeCodeProvider _eventTypeCodeProvider;
        private readonly Producer _producer;

        public Producer Producer { get { return _producer; } }

        public EventPublisher() : this(DefaultEventPublisherProcuderId) { }
        public EventPublisher(string id) : this(id, new ProducerSetting()) { }
        public EventPublisher(string id, ProducerSetting setting)
        {
            _producer = new Producer(id, setting);
            _binarySerializer = ObjectContainer.Resolve<IBinarySerializer>();
            _eventTopicProvider = ObjectContainer.Resolve<IEventTopicProvider>();
            _eventTypeCodeProvider = ObjectContainer.Resolve<IEventTypeCodeProvider>();
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

        public void PublishEvent(IDictionary<string, string> contextItems, EventStream eventStream)
        {
            var eventMessage = ConvertToData(contextItems, eventStream);
            var topic = _eventTopicProvider.GetTopic(eventStream);
            var data = _binarySerializer.Serialize(eventMessage);
            var message = new Message(topic, data);
            var result = _producer.Send(message, eventStream.AggregateRootId);
            if (result.SendStatus != SendStatus.Success)
            {
                throw new ENodeException("Publish event failed, eventStream:[{0}]", eventStream);
            }
        }

        private EventMessage ConvertToData(IDictionary<string, string> contextItems, EventStream eventStream)
        {
            var data = new EventMessage();

            data.CommitId = eventStream.CommitId;
            data.AggregateRootId = eventStream.AggregateRootId;
            data.AggregateRootTypeCode = eventStream.AggregateRootTypeCode;
            data.Timestamp = eventStream.Timestamp;
            data.ProcessId = eventStream.ProcessId;
            data.Version = eventStream.Version;
            data.Items = eventStream.Items;
            data.ContextItems = contextItems;

            foreach (var evnt in eventStream.Events)
            {
                var typeCode = _eventTypeCodeProvider.GetTypeCode(evnt.GetType());
                var eventData = _binarySerializer.Serialize(evnt);
                data.Events.Add(new EventEntry(typeCode, eventData));
            }

            return data;
        }
    }
}
