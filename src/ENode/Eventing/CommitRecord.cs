using System;

namespace ENode.Eventing
{
    [Serializable]
    public class CommitRecord
    {
        public CommitRecord(long commitSequence, string commitId, string aggregateRootId, int version)
        {
            CommitSequence = commitSequence;
            CommitId = commitId;
            AggregateRootId = aggregateRootId;
            Version = version;
        }

        public long CommitSequence { get; private set; }
        public string CommitId { get; private set; }
        public string AggregateRootId { get; private set; }
        public int Version { get; private set; }
    }
}
