using System;

namespace ENode.Infrastructure
{
    public class TwoMessageHandleRecord
    {
        public string MessageId1 { get; set; }
        public string MessageId2 { get; set; }
        public string Message1TypeName { get; set; }
        public string Message2TypeName { get; set; }
        public string HandlerTypeName { get; set; }
        public string AggregateRootTypeName { get; set; }
        public string AggregateRootId { get; set; }
        public int Version { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
