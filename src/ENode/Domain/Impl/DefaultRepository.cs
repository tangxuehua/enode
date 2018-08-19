using System;
using System.Threading.Tasks;

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

        public async Task<T> GetAsync<T>(object aggregateRootId) where T : class, IAggregateRoot
        {
            return await GetAsync(typeof(T), aggregateRootId) as T;
        }
        public async Task<IAggregateRoot> GetAsync(Type aggregateRootType, object aggregateRootId)
        {
            if (aggregateRootType == null)
            {
                throw new ArgumentNullException("aggregateRootType");
            }
            if (aggregateRootId == null)
            {
                throw new ArgumentNullException("aggregateRootId");
            }
            return await _memoryCache.GetAsync(aggregateRootId, aggregateRootType) ?? await _aggregateRootStorage.GetAsync(aggregateRootType, aggregateRootId.ToString());
        }
    }
}
