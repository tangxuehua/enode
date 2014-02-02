using System;
using System.Collections.Generic;
using EQueue.Utils;

namespace ENode.EQueue
{
    [Serializable]
    public class EventStreamData
    {
        public Guid CommandId { get; set; }
        public object AggregateRootId { get; set; }
        public string AggregateRootName { get; set; }
        public long Version { get; set; }
        public DateTime Timestamp { get; set; }
        public bool HasProcessCompletedEvent { get; set; }
        public IList<ByteTypeData> Events { get; set; }

        public EventStreamData()
        {
            Events = new List<ByteTypeData>();
        }
    }
}
