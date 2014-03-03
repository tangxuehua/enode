using System;
using System.Collections.Generic;

namespace ENode.Eventing
{
    /// <summary>The event stream contains the binary serialized domain events.
    /// </summary>
    [Serializable]
    public class EventByteStream
    {
        public string CommitId { get; private set; }
        public string AggregateRootId { get; private set; }
        public string AggregateRootName { get; private set; }
        public int Version { get; private set; }
        public DateTime Timestamp { get; private set; }
        public IEnumerable<EventEntry> Events { get; private set; }

        public EventByteStream(string commitId, string aggregateRootId, string aggregateRootName, int version, DateTime timestamp, IEnumerable<EventEntry> events)
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
