using System;
using System.Collections.Generic;
using EQueue.Utils;

namespace ENode.EQueue
{
    [Serializable]
    public class EventMessage
    {
        public Guid CommandId { get; set; }
        public string AggregateRootId { get; set; }
        public string AggregateRootName { get; set; }
        public long Version { get; set; }
        public DateTime Timestamp { get; set; }
        public IList<ByteTypeData> Events { get; set; }
        public IDictionary<string, object> ContextItems { get; set; }

        public EventMessage()
        {
            Events = new List<ByteTypeData>();
        }
    }
}
