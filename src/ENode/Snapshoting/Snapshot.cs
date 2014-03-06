using System;

namespace ENode.Snapshoting
{
    /// <summary>Snapshot of aggregate.
    /// </summary>
    public class Snapshot
    {
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="aggregateRootTypeCode"></param>
        /// <param name="aggregateRootId"></param>
        /// <param name="version"></param>
        /// <param name="payload"></param>
        /// <param name="timestamp"></param>
        public Snapshot(int aggregateRootTypeCode, string aggregateRootId, int version, byte[] payload, DateTime timestamp)
        {
            AggregateRootTypeCode = aggregateRootTypeCode;
            AggregateRootId = aggregateRootId;
            Version = version;
            Payload = payload;
            Timestamp = timestamp;
        }

        /// <summary>The aggregate root id.
        /// </summary>
        public string AggregateRootId { get; set; }
        /// <summary>The aggregate root type code.
        /// </summary>
        public int AggregateRootTypeCode { get; set; }
        /// <summary>The aggregate root version when creating this snapshot.
        /// </summary>
        public int Version { get; set; }
        /// <summary>The aggregate root payload data when creating this snapshot.
        /// </summary>
        public byte[] Payload { get; set; }
        /// <summary>The created time of this snapshot.
        /// </summary>
        public DateTime Timestamp { get; set; }
    }
}
