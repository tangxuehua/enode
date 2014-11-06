using System;
using System.Collections.Generic;
using ENode.Eventing;

namespace ENode.EQueue
{
    [Serializable]
    public class DomainEventStreamMessage : EventStreamMessage
    {
        public string AggregateRootId { get; set; }
        public int AggregateRootTypeCode { get; set; }
        public int Version { get; set; }
        public DateTime Timestamp { get; set; }
        public IEnumerable<IDomainEvent> DomainEvents { get; set; }

        public DomainEventStreamMessage()
        {
            Events = new List<IDomainEvent>();
        }
    }
}
