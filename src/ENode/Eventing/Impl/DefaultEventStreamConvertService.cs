using System.Collections.Generic;
using ECommon.Serializing;

namespace ENode.Eventing.Impl
{
    public class DefaultEventStreamConvertService : IEventStreamConvertService
    {
        private IEventTypeCodeProvider _eventTypeCodeProvider;
        private IBinarySerializer _binarySerializer;

        public DefaultEventStreamConvertService(IEventTypeCodeProvider eventTypeCodeProvider, IBinarySerializer binarySerializer)
        {
            _eventTypeCodeProvider = eventTypeCodeProvider;
            _binarySerializer = binarySerializer;
        }

        public EventByteStream ConvertTo(EventStream source)
        {
            if (source == null) return null;

            var eventEntryList = new List<EventEntry>();

            foreach (var evnt in source.Events)
            {
                var typeCode = _eventTypeCodeProvider.GetTypeCode(evnt);
                var eventData = _binarySerializer.Serialize(evnt);
                eventEntryList.Add(new EventEntry(typeCode, eventData));
            }

            return new EventByteStream(source.CommandId, source.AggregateRootId, source.AggregateRootName, source.Version, source.Timestamp, eventEntryList);
        }
        public EventStream ConvertFrom(EventByteStream source)
        {
            if (source == null) return null;

            var eventList = new List<IDomainEvent>();

            foreach (var entry in source.Events)
            {
                var eventType = _eventTypeCodeProvider.GetType(entry.EventTypeCode);
                var domainEvent = _binarySerializer.Deserialize(entry.EventData, eventType) as IDomainEvent;
                eventList.Add(domainEvent);
            }

            return new EventStream(source.CommandId, source.AggregateRootId, source.AggregateRootName, source.Version, source.Timestamp, eventList);
        }
    }
}
