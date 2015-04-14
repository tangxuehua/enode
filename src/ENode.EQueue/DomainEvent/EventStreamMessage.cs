using System;
using System.Collections.Generic;

namespace ENode.EQueue
{
    [Serializable]
    public class EventStreamMessage
    {
        public string AggregateRootId { get; set; }
        public int AggregateRootTypeCode { get; set; }
        public int Version { get; set; }
        public DateTime Timestamp { get; set; }
        public string CommandId { get; set; }
        public IDictionary<int, string> Events { get; set; }
        public IDictionary<string, string> Items { get; set; }
    }
}
