using System;
using System.Collections.Generic;

namespace ENode.EQueue
{
    [Serializable]
    public class EventStreamMessage
    {
        public string AggregateRootId { get; set; }
        public string AggregateRootTypeName { get; set; }
        public int Version { get; set; }
        public DateTime Timestamp { get; set; }
        public string CommandId { get; set; }
        public IDictionary<string, string> Events { get; set; }
        public IDictionary<string, string> Items { get; set; }
    }
}
