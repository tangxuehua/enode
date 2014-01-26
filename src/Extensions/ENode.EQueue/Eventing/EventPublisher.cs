using System;
using ECommon.IoC;
using ECommon.Logging;
using ECommon.Serializing;
using ECommon.Socketing;
using ENode.Eventing;
using EQueue.Clients.Producers;
using EQueue.Protocols;
using EQueue.Utils;

namespace ENode.EQueue
{
    public class EventPublisher : IEventPublisher
    {
        private readonly ILogger _logger;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IBinarySerializer _binarySerializer;
        private readonly IEventTopicProvider _eventTopicProvider;
        private readonly IEventTypeCodeProvider _eventTypeCodeProvider;
        private readonly Producer _producer;

        public EventPublisher() : this(ProducerSetting.Default) { }
        public EventPublisher(ProducerSetting setting) : this(string.Format("EventPublisher@{0}", SocketUtils.GetLocalIPV4()), setting) { }
        public EventPublisher(string id, ProducerSetting setting)
        {
            _producer = new Producer(id, setting);
            _jsonSerializer = ObjectContainer.Resolve<IJsonSerializer>();
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

        public void PublishEvent(EventStream eventStream)
        {
            var eventStreamData = ConvertToData(eventStream);
            var topic = _eventTopicProvider.GetTopic(eventStream);
            var data = _binarySerializer.Serialize(eventStreamData);
            var message = new Message(topic, data);
            var result = _producer.Send(message, eventStream.AggregateRootId);
            if (result.SendStatus != SendStatus.Success)
            {
                throw new Exception(string.Format("Publish event failed, eventStream:[{0}]", eventStream));
            }
        }

        private EventStreamData ConvertToData(EventStream eventStream)
        {
            var data = new EventStreamData();

            data.Id = eventStream.Id;
            data.AggregateRootId = eventStream.AggregateRootId;
            data.AggregateRootName = eventStream.AggregateRootName;
            data.CommandId = eventStream.CommandId;
            data.Timestamp = eventStream.Timestamp;
            data.Version = eventStream.Version;

            foreach (var evnt in eventStream.Events)
            {
                var typeCode = _eventTypeCodeProvider.GetTypeCode(evnt);
                var eventData = _jsonSerializer.Serialize(evnt);
                data.Events.Add(new StringTypeData(typeCode, eventData));
            }

            return data;
        }
    }
}
