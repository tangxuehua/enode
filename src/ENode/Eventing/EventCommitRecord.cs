using System;
using System.Collections.Generic;

namespace ENode.Eventing
{
    /// <summary>The event commit record represents a single commit of domain events of one aggregate.
    /// </summary>
    [Serializable]
    public class EventCommitRecord
    {
        public string CommitId { get; private set; }
        public string AggregateRootId { get; private set; }
        public string AggregateRootName { get; private set; }
        public int Version { get; private set; }
        public DateTime Timestamp { get; private set; }
        public IEnumerable<EventEntry> Events { get; private set; }

        public EventCommitRecord(string commitId, string aggregateRootId, string aggregateRootName, int version, DateTime timestamp, IEnumerable<EventEntry> events)
        {
            CommitId = commitId;
            AggregateRootId = aggregateRootId;
            AggregateRootName = aggregateRootName;
            Version = version;
            Timestamp = timestamp;
            Events = events;
        }
    }
    [Serializable]
    public class EventEntry
    {
        public int EventTypeCode { get; private set; }
        public byte[] EventData { get; private set; }

        public EventEntry(int eventTypeCode, byte[] eventData)
        {
            EventTypeCode = eventTypeCode;
            EventData = eventData;
        }
    }
}
