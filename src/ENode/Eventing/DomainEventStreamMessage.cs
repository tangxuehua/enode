using System;
using System.Collections.Generic;
using System.Linq;
using ENode.Infrastructure;

namespace ENode.Eventing
{
    [Serializable]
    public class DomainEventStreamMessage : SequenceMessage<string>
    {
        public string CommandId { get; set; }
        public IDictionary<string, string> Items { get; set; }
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
            return string.Format("[MessageId={0},CommandId={1},AggregateRootId={2},AggregateRootTypeName={3},Version={4},Events={5},Items={6}]",
                Id,
                CommandId,
                AggregateRootId,
                AggregateRootTypeName,
                Version,
                string.Join("|", Events.Select(x => x.GetType().Name)),
                string.Join("|", Items.Select(x => x.Key + ":" + x.Value)));
        }
    }
}
