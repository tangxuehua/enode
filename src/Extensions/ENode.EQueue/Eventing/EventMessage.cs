using System;
using System.Collections.Generic;
using ENode.Eventing;
using EQueue.Utils;

namespace ENode.EQueue
{
    [Serializable]
    public class EventMessage
    {
        public string CommitId { get; set; }
        public string AggregateRootId { get; set; }
        public int AggregateRootTypeCode { get; set; }
        public string ProcessId { get; set; }
        public int Version { get; set; }
        public DateTime Timestamp { get; set; }
        public IList<EventEntry> Events { get; set; }
        public IDictionary<string, string> Items { get; set; }
        public IDictionary<string, string> ContextItems { get; set; }

        public EventMessage()
        {
            Events = new List<EventEntry>();
        }
    }
}
