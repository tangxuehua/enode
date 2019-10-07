using System;
using System.Collections.Generic;

namespace ENode.EQueue
{
    [Serializable]
    public class DomainExceptionMessage
    {
        public string UniqueId { get; set; }
        public DateTime Timestamp { get; set; }
        public IDictionary<string, string> Items { get; set; }
        public IDictionary<string, string> SerializableInfo { get; set; }
    }
}
