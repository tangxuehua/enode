using System;
using System.Collections.Generic;

namespace ENode.EQueue
{
    public class EventStreamData
    {
        public Guid Id { get; set; }
        public object AggregateRootId { get; set; }
        public string AggregateRootName { get; set; }
        public Guid CommandId { get; set; }
        public long Version { get; set; }
        public DateTime Timestamp { get; set; }
        public IList<TypeData> Events { get; set; }

        public EventStreamData()
        {
            Events = new List<TypeData>();
        }
    }
}
