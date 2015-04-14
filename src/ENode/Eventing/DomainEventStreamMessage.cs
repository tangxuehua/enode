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
        public DomainEventStreamMessage(string commandId, string aggregateRootId, int version, int aggregateRootTypeCode, IEnumerable<IDomainEvent> events, IDictionary<string, string> items)
            : base(aggregateRootId, version)
        {
            CommandId = commandId;
            AggregateRootTypeCode = aggregateRootTypeCode;
            Events = events;
            Items = items;
        }

        public override string ToString()
        {
            return string.Format("[MessageId={0},CommandId={1},AggregateRootId={2},AggregateRootTypeCode={3},Version={4},Events={5},Items={6}]",
                Id,
                CommandId,
                AggregateRootId,
                AggregateRootTypeCode,
                Version,
                string.Join("|", Events.Select(x => x.GetType().Name)),
                string.Join("|", Items.Select(x => x.Key + ":" + x.Value)));
        }
    }
}
