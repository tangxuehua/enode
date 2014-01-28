using System;
using System.Threading;
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
        private static int _eventPublisherIndex;
        private readonly ILogger _logger;
        private readonly IBinarySerializer _binarySerializer;
        private readonly IEventTopicProvider _eventTopicProvider;
        private readonly IEventTypeCodeProvider _eventTypeCodeProvider;
        private readonly Producer _producer;

        public EventPublisher() : this(ProducerSetting.Default) { }
        public EventPublisher(ProducerSetting setting) : this(string.Format("{0}@{1}-{2}-{3}", SocketUtils.GetLocalIPV4(), typeof(EventPublisher).Name, Interlocked.Increment(ref _eventPublisherIndex), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ffff")), setting) { }
        public EventPublisher(string id, ProducerSetting setting)
        {
            _producer = new Producer(id, setting);
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
            data.HasProcessCompletedEvent = eventStream.HasProcessCompletedEvent;

            foreach (var evnt in eventStream.Events)
            {
                var typeCode = _eventTypeCodeProvider.GetTypeCode(evnt);
                var eventData = _binarySerializer.Serialize(evnt);
                data.Events.Add(new ByteTypeData(typeCode, eventData));
            }

            return data;
        }
    }
}
