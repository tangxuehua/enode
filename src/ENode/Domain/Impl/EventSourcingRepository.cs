using System;
using ENode.Commanding;
using ENode.Infrastructure;

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
            try
            {
                var aggregateRoot = _memoryCache.Get(aggregateRootId, aggregateRootType) ?? _aggregateRootStorage.Get(aggregateRootType, aggregateRootId.ToString());
                if (aggregateRoot == null)
                {
                    throw new AggregateRootNotExistException(aggregateRootId, aggregateRootType);
                }
                return aggregateRoot;
            }
            catch (Exception ex)
            {
                throw new ENodeException(string.Format("Get aggregate from repoisotry has exception, aggregateRootType:{0}, aggregateRootId:{1}", aggregateRootType, aggregateRootId), ex);
            }
        }
    }
}
