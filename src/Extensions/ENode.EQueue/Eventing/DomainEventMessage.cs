using System;
using System.Collections.Generic;
using ENode.Eventing;

namespace ENode.EQueue
{
    [Serializable]
    public class DomainEventMessage
    {
        public string CommandId { get; set; }
        public string AggregateRootId { get; set; }
        public int AggregateRootTypeCode { get; set; }
        public string ProcessId { get; set; }
        public int Version { get; set; }
        public DateTime Timestamp { get; set; }
        public IEnumerable<IDomainEvent> Events { get; set; }
        public IDictionary<string, string> Items { get; set; }
        public IDictionary<string, string> ContextItems { get; set; }

        public DomainEventMessage()
        {
            Events = new List<IDomainEvent>();
        }
    }
}
