using System;

namespace ENode.Domain
{
    public class AggregateCacheInfo
    {
        public IAggregateRoot AggregateRoot { get; private set; }
        public DateTime LastUpdateTime { get; private set; }

        public AggregateCacheInfo(IAggregateRoot aggregateRoot)
        {
            AggregateRoot = aggregateRoot;
            LastUpdateTime = DateTime.Now;
        }

        public void UpdateAggregateRoot(IAggregateRoot aggregateRoot)
        {
            AggregateRoot = aggregateRoot;
            LastUpdateTime = DateTime.Now;
        }
        public bool IsExpired(int timeoutSeconds)
        {
            return (DateTime.Now - LastUpdateTime).TotalSeconds >= timeoutSeconds;
        }
    }
}
