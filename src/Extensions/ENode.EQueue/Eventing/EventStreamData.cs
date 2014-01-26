using System;
using System.Collections.Generic;
using EQueue.Utils;

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
        public IList<StringTypeData> Events { get; set; }

        public EventStreamData()
        {
            Events = new List<StringTypeData>();
        }
    }
}
