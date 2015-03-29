using System;

namespace ENode.Infrastructure
{
    public class MessageHandleRecord
    {
        public string MessageId { get; set; }
        public int HandlerTypeCode { get; set; }
        public int MessageTypeCode { get; set; }
        public int AggregateRootTypeCode { get; set; }
        public string AggregateRootId { get; set; }
        public int Version { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
