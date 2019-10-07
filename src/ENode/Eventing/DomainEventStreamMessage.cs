using System;
using System.Collections.Generic;
using System.Linq;
using ENode.Messaging;

namespace ENode.Eventing
{
    [Serializable]
    public class DomainEventStreamMessage : Message
    {
        public string AggregateRootId { get; set; }
        public string AggregateRootTypeName { get; set; }
        public int Version { get; set; }
        public string CommandId { get; set; }
        public IEnumerable<IDomainEvent> Events { get; set; }

        public DomainEventStreamMessage() { }
        public DomainEventStreamMessage(string commandId, string aggregateRootId, int version, string aggregateRootTypeName, IEnumerable<IDomainEvent> events, IDictionary<string, string> items)
        {
            CommandId = commandId;
            AggregateRootId = aggregateRootId;
            Version = version;
            AggregateRootTypeName = aggregateRootTypeName;
            Events = events;
            Items = items;
        }

        public override string ToString()
        {
            return string.Format("[Id={0},CommandId={1},AggregateRootId={2},AggregateRootTypeName={3},Version={4},Events={5},Items={6},Timestamp={7}]",
                Id,
                CommandId,
                AggregateRootId,
                AggregateRootTypeName,
                Version,
                string.Join("|", Events.Select(x => x.GetType().Name)),
                string.Join("|", Items.Select(x => x.Key + ":" + x.Value)),
                Timestamp);
        }
    }
}
