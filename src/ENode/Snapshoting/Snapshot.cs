using System;

namespace ENode.Snapshoting
{
    /// <summary>Snapshot of aggregate.
    /// </summary>
    public class Snapshot
    {
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="aggregateRootName"></param>
        /// <param name="aggregateRootId"></param>
        /// <param name="version"></param>
        /// <param name="payload"></param>
        /// <param name="timestamp"></param>
        public Snapshot(string aggregateRootName, string aggregateRootId, long version, object payload, DateTime timestamp)
        {
            AggregateRootName = aggregateRootName;
            AggregateRootId = aggregateRootId;
            Version = version;
            Payload = payload;
            Timestamp = timestamp;
        }

        /// <summary>The aggregate root id.
        /// </summary>
        public string AggregateRootId { get; set; }
        /// <summary>The aggregate root name.
        /// </summary>
        public string AggregateRootName { get; set; }
        /// <summary>The aggregate root version when creating this snapshot.
        /// </summary>
        public long Version { get; set; }
        /// <summary>The aggregate root payload data when creating this snapshot.
        /// </summary>
        public object Payload { get; set; }
        /// <summary>The created time of this snapshot.
        /// </summary>
        public DateTime Timestamp { get; set; }
    }
}
