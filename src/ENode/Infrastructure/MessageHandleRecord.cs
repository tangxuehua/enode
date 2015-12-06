using System;

namespace ENode.Infrastructure
{
    public class MessageHandleRecord
    {
        public string MessageId { get; set; }
        public string HandlerTypeName { get; set; }
        public string MessageTypeName { get; set; }
        public string AggregateRootTypeName { get; set; }
        public string AggregateRootId { get; set; }
        public int Version { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
