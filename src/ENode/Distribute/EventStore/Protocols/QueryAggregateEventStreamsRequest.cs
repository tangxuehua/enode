using System;

namespace ENode.Distribute.EventStore.Protocols
{
    [Serializable]
    public class QueryAggregateEventStreamsRequest
    {
        public string AggregateRootId { get; private set; }
        public string AggregateRootName { get; private set; }
        public int MinVersion { get; private set; }
        public int MaxVersion { get; private set; }

        public QueryAggregateEventStreamsRequest(string aggregateRootId, string aggregateRootName, int minVersion, int maxVersion)
        {
            AggregateRootId = aggregateRootId;
            AggregateRootName = aggregateRootName;
            MinVersion = minVersion;
            MaxVersion = maxVersion;
        }

        public override string ToString()
        {
            return string.Format("[AggregateRootId:{0}, AggregateRootName:{1}, MinVersion:{2}, MaxVersion:{3}]", AggregateRootId, AggregateRootName, MinVersion, MaxVersion);
        }
    }
}
