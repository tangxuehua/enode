using System;

namespace ENode.Snapshoting {
    /// <summary>Snapshot of aggregate.
    /// </summary>
    public class Snapshot {
        public Snapshot(string aggregateRootName, string aggregateRootId, long streamVersion, object payload, DateTime timestamp) {
            AggregateRootName = aggregateRootName;
            AggregateRootId = aggregateRootId;
            StreamVersion = streamVersion;
            Payload = payload;
            Timestamp = timestamp;
        }

        public string AggregateRootId { get; set; }
        public string AggregateRootName { get; set; }
        public long StreamVersion { get; set; }
        public object Payload { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
