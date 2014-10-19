using System;
using ENode.Commanding;

namespace ENode.Domain.Impl
{
    public class EventSourcingRepository : IRepository
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IAggregateStorage _aggregateRootStorage;

        public EventSourcingRepository(IMemoryCache memoryCache, IAggregateStorage aggregateRootStorage)
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
            var aggregateRoot = _memoryCache.Get(aggregateRootId, aggregateRootType) ?? _aggregateRootStorage.Get(aggregateRootType, aggregateRootId.ToString());
            if (aggregateRoot == null)
            {
                throw new AggregateRootNotExistException(aggregateRootId, aggregateRootType);
            }
            return aggregateRoot;
        }
    }
}
