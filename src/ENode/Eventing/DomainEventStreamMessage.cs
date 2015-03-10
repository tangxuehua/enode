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

        public DomainEventStreamMessage(string commandId, string aggregateRootId, int version, IEnumerable<IDomainEvent> events, IDictionary<string, string> items)
            : base(aggregateRootId, version)
        {
            CommandId = commandId;
            Events = events;
            Items = items;
        }

        public DomainEventStreamMessage()
        {
            // TODO: Complete member initialization
        }

        public override string ToString()
        {
            return string.Format("[MessageId={0},CommandId={1},AggregateRootId={2},Version={3},Events={4},Items={5}]",
                Id,
                CommandId,
                AggregateRootId,
                Version,
                string.Join("|", Events.Select(x => x.GetType().Name)),
                string.Join("|", Items.Select(x => x.Key + ":" + x.Value)));
        }
    }
}
