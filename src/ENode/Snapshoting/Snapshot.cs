using System;

namespace ENode.Snapshoting
{
    /// <summary>Snapshot of aggregate.
    /// </summary>
    public class Snapshot
    {
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="aggregateRootTypeName"></param>
        /// <param name="aggregateRootId"></param>
        /// <param name="version"></param>
        /// <param name="payload"></param>
        /// <param name="createdOn"></param>
        public Snapshot(string aggregateRootTypeName, string aggregateRootId, int version, byte[] payload, DateTime createdOn)
        {
            AggregateRootTypeName = aggregateRootTypeName;
            AggregateRootId = aggregateRootId;
            Version = version;
            Payload = payload;
            CreatedOn = createdOn;
        }

        /// <summary>The aggregate root id.
        /// </summary>
        public string AggregateRootId { get; set; }
        /// <summary>The aggregate root type name.
        /// </summary>
        public string AggregateRootTypeName { get; set; }
        /// <summary>The aggregate root version when creating this snapshot.
        /// </summary>
        public int Version { get; set; }
        /// <summary>The aggregate root payload data when creating this snapshot.
        /// </summary>
        public byte[] Payload { get; set; }
        /// <summary>The created time of this snapshot.
        /// </summary>
        public DateTime CreatedOn { get; set; }
    }
}
