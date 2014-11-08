using System;
using System.Collections.Concurrent;

namespace ENode.Domain.Impl
{
    public class DefaultMemoryCache : IMemoryCache
    {
        private readonly ConcurrentDictionary<string, IAggregateRoot> _aggregateRootDict = new ConcurrentDictionary<string, IAggregateRoot>();

        public IAggregateRoot Get(object aggregateRootId, Type aggregateRootType)
        {
            if (aggregateRootId == null) throw new ArgumentNullException("aggregateRootId");
            IAggregateRoot aggregateRoot;
            if (_aggregateRootDict.TryGetValue(aggregateRootId.ToString(), out aggregateRoot))
            {
                if (aggregateRoot.GetType() != aggregateRootType)
                {
                    throw new Exception(string.Format("Incorrect aggregate root type, found aggregateRoot's id:{0}, type:{1}, expecting aggregateRoot's type:{2}", aggregateRootId, aggregateRoot.GetType(), aggregateRootType));
                }
                aggregateRoot.ClearUncommittedEvents();
                return aggregateRoot;
            }
            return null;
        }
        public T Get<T>(object aggregateRootId) where T : class, IAggregateRoot
        {
            return Get(aggregateRootId, typeof(T)) as T;
        }
        public void Set(IAggregateRoot aggregateRoot)
        {
            if (aggregateRoot == null)
            {
                throw new ArgumentNullException("aggregateRoot");
            }
            _aggregateRootDict[aggregateRoot.UniqueId] = aggregateRoot;
        }
    }
}
