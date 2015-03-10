using System.Collections.Generic;
using ECommon.Serializing;
using ENode.Infrastructure;

namespace ENode.Eventing.Impl
{
    public class DefaultEventSerializer : IEventSerializer
    {
        private readonly ITypeCodeProvider _typeCodeProvider;
        private readonly IJsonSerializer _jsonSerializer;

        public DefaultEventSerializer(ITypeCodeProvider typeCodeProvider, IJsonSerializer jsonSerializer)
        {
            _typeCodeProvider = typeCodeProvider;
            _jsonSerializer = jsonSerializer;
        }

        public IDictionary<int, string> Serialize(IEnumerable<IDomainEvent> evnts)
        {
            var dict = new Dictionary<int, string>();

            foreach (var evnt in evnts)
            {
                var typeCode = _typeCodeProvider.GetTypeCode(evnt.GetType());
                var eventData = _jsonSerializer.Serialize(evnt);
                dict.Add(typeCode, eventData);
            }

            return dict;
        }
        public IEnumerable<TEvent> Deserialize<TEvent>(IDictionary<int, string> data) where TEvent : class, IDomainEvent
        {
            var evnts = new List<TEvent>();

            foreach (var entry in data)
            {
                var eventType = _typeCodeProvider.GetType(entry.Key);
                var evnt = _jsonSerializer.Deserialize(entry.Value, eventType) as TEvent;
                evnts.Add(evnt);
            }

            return evnts;
        }
    }
}
