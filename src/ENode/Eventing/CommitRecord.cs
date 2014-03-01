using System;

namespace ENode.Eventing
{
    [Serializable]
    public class CommitRecord
    {
        public CommitRecord(long commitSequence, Guid commandId, string aggregateRootId, long version)
        {
            CommitSequence = commitSequence;
            CommandId = commandId;
            AggregateRootId = aggregateRootId;
            Version = version;
        }

        public long CommitSequence { get; private set; }
        public Guid CommandId { get; private set; }
        public string AggregateRootId { get; private set; }
        public long Version { get; private set; }
    }
}
