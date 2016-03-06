using System;

namespace ENode.Domain
{
    public class AggregateCacheInfo
    {
        public IAggregateRoot AggregateRoot;
        public DateTime LastUpdateTime;

        public AggregateCacheInfo(IAggregateRoot aggregateRoot)
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
