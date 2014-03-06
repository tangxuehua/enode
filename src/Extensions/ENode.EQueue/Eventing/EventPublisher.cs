using System.Collections.Generic;
using ECommon.IoC;
using ECommon.Logging;
using ECommon.Serializing;
using ECommon.Socketing;
using ECommon.Utilities;
using ENode.Eventing;
using ENode.Infrastructure;
using EQueue.Clients.Producers;
using EQueue.Protocols;

namespace ENode.EQueue
{
    public class EventPublisher : IEventPublisher
    {
        private readonly ILogger _logger;
        private readonly IBinarySerializer _binarySerializer;
        private readonly IEventTopicProvider _eventTopicProvider;
        private readonly IEventTypeCodeProvider _eventTypeCodeProvider;
        private readonly Producer _producer;

        public Producer Producer { get { return _producer; } }

        public EventPublisher() : this(new ProducerSetting()) { }
        public EventPublisher(ProducerSetting setting) : this(null, setting) { }
        public EventPublisher(string name, ProducerSetting setting) : this(setting, string.Format("{0}@{1}@{2}", SocketUtils.GetLocalIPV4(), string.IsNullOrEmpty(name) ? typeof(EventPublisher).Name : name, ObjectId.GenerateNewId())) { }
        public EventPublisher(ProducerSetting setting, string id)
        {
            _producer = new Producer(setting, id);
            _binarySerializer = ObjectContainer.Resolve<IBinarySerializer>();
            _eventTopicProvider = ObjectContainer.Resolve<IEventTopicProvider>();
            _eventTypeCodeProvider = ObjectContainer.Resolve<IEventTypeCodeProvider>();
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().Name);
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
            data.Version = eventStream.Version;
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
