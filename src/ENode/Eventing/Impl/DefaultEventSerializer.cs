using System;
using System.Collections.Generic;
using ECommon.Serializing;
using ENode.Infrastructure;
using ENode.Infrastructure.Impl;

namespace ENode.Eventing.Impl
{
    public class DefaultEventSerializer : IEventSerializer
    {
        private readonly ITypeCodeProvider<IEvent> _eventTypeCodeProvider;
        private readonly IJsonSerializer _jsonSerializer;

        public DefaultEventSerializer(ITypeCodeProvider<IEvent> eventTypeCodeProvider, IJsonSerializer jsonSerializer)
        {
            _eventTypeCodeProvider = eventTypeCodeProvider;
            _jsonSerializer = jsonSerializer;
        }

        public IDictionary<int, string> Serialize(IEnumerable<IEvent> evnts)
        {
            var dict = new Dictionary<int, string>();

            foreach (var evnt in evnts)
            {
                var typeCode = _eventTypeCodeProvider.GetTypeCode(evnt.GetType());
                var eventData = _jsonSerializer.Serialize(evnt);
                dict.Add(typeCode, eventData);
            }

            return dict;
        }
        public IEnumerable<TEvent> Deserialize<TEvent>(IDictionary<int, string> data) where TEvent : class, IEvent
        {
            var evnts = new List<TEvent>();

            foreach (var entry in data)
            {
                var eventType = _eventTypeCodeProvider.GetType(entry.Key);
                var evnt = _jsonSerializer.Deserialize(entry.Value, eventType) as TEvent;
                evnts.Add(evnt);
            }

            return evnts;
        }
    }
}
