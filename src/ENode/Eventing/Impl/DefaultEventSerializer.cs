using System.Collections.Generic;
using ECommon.Serializing;
using ENode.Infrastructure;

namespace ENode.Eventing.Impl
{
    public class DefaultEventSerializer : IEventSerializer
    {
        private readonly ITypeNameProvider _typeNameProvider;
        private readonly IJsonSerializer _jsonSerializer;

        public DefaultEventSerializer(ITypeNameProvider typeNameProvider, IJsonSerializer jsonSerializer)
        {
            _typeNameProvider = typeNameProvider;
            _jsonSerializer = jsonSerializer;
        }

        public IDictionary<string, string> Serialize(IEnumerable<IDomainEvent> evnts)
        {
            var dict = new Dictionary<string, string>();

            foreach (var evnt in evnts)
            {
                var typeName = _typeNameProvider.GetTypeName(evnt.GetType());
                var eventData = _jsonSerializer.Serialize(evnt);
                dict.Add(typeName, eventData);
            }

            return dict;
        }
        public IEnumerable<TEvent> Deserialize<TEvent>(IDictionary<string, string> data) where TEvent : class, IDomainEvent
        {
            var evnts = new List<TEvent>();

            foreach (var entry in data)
            {
                var eventType = _typeNameProvider.GetType(entry.Key);
                var evnt = _jsonSerializer.Deserialize(entry.Value, eventType) as TEvent;
                evnts.Add(evnt);
            }

            return evnts;
        }
    }
}
