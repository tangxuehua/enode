using System;

namespace ENode.Infrastructure
{
    public class TwoMessageHandleRecord
    {
        public string MessageId1 { get; set; }
        public string MessageId2 { get; set; }
        public int Message1TypeCode { get; set; }
        public int Message2TypeCode { get; set; }
        public int HandlerTypeCode { get; set; }
        public int AggregateRootTypeCode { get; set; }
        public string AggregateRootId { get; set; }
        public int Version { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
