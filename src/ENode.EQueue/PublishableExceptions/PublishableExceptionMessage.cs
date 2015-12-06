using System;
using System.Collections.Generic;

namespace ENode.EQueue
{
    [Serializable]
    public class PublishableExceptionMessage
    {
        public string UniqueId { get; set; }
        public string AggregateRootId { get; set; }
        public string AggregateRootTypeName { get; set; }
        public DateTime Timestamp { get; set; }
        public IDictionary<string, string> SerializableInfo { get; set; }
    }
}
