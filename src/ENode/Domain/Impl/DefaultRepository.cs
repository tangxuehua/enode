using System;

namespace ENode.Domain.Impl
{
    public class DefaultRepository : IRepository
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IAggregateStorage _aggregateRootStorage;

        public DefaultRepository(IMemoryCache memoryCache, IAggregateStorage aggregateRootStorage)
        {
            _memoryCache = memoryCache;
            _aggregateRootStorage = aggregateRootStorage;
        }

        public T Get<T>(object aggregateRootId) where T : class, IAggregateRoot
        {
            return Get(typeof(T), aggregateRootId) as T;
        }
        public IAggregateRoot Get(Type aggregateRootType, object aggregateRootId)
        {
            if (aggregateRootType == null)
            {
                throw new ArgumentNullException("aggregateRootType");
            }
            if (aggregateRootId == null)
            {
                throw new ArgumentNullException("aggregateRootId");
            }
            return _memoryCache.Get(aggregateRootId, aggregateRootType) ?? _aggregateRootStorage.Get(aggregateRootType, aggregateRootId.ToString());
        }
    }
}
