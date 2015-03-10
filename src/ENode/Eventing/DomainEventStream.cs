using System;
using System.Collections.Generic;
using System.Linq;

namespace ENode.Eventing
{
    [Serializable]
    public class DomainEventStream
    {
        public DomainEventStream(string commandId, string aggregateRootId, int aggregateRootTypeCode, int version, DateTime timestamp, IEnumerable<IDomainEvent> events, IDictionary<string, string> items = null)
        {
            CommandId = commandId;
            AggregateRootId = aggregateRootId;
            AggregateRootTypeCode = aggregateRootTypeCode;
            Version = version;
            Timestamp = timestamp;
            Events = events;
            Items = items ?? new Dictionary<string, string>();
        }

        public string CommandId { get; private set; }
        public int AggregateRootTypeCode { get; private set; }
        public string AggregateRootId { get; private set; }
        public int Version { get; private set; }
        public IEnumerable<IDomainEvent> Events { get; private set; }
        public DateTime Timestamp { get; private set; }
        public IDictionary<string, string> Items { get; internal set; }

        public override string ToString()
        {
            var format = "[CommandId={0},AggregateRootTypeCode={1},AggregateRootId={2},Version={3},Timestamp={4},Events={5},Items={6}]";
            return string.Format(format,
                CommandId,
                AggregateRootTypeCode,
                AggregateRootId,
                Version,
                Timestamp,
                string.Join("|", Events.Select(x => x.GetType().Name)),
                string.Join("|", Items.Select(x => x.Key + ":" + x.Value)));
        }
    }
}
