using System;

namespace ENode.Distribute.EventStore.Protocols
{
    [Serializable]
    public class GetEventStreamRequest
    {
        public string AggregateRootId { get; private set; }
        public string CommitId { get; private set; }

        public GetEventStreamRequest(string aggregateRootId, string commitId)
        {
            AggregateRootId = aggregateRootId;
            CommitId = commitId;
        }

        public override string ToString()
        {
            return string.Format("[AggregateRootId:{0}, CommitId:{1}]", AggregateRootId, CommitId);
        }
    }
}
