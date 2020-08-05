using System;
using System.Threading.Tasks;

namespace ENode.Domain.Impl
{
    public class DefaultRepository : IRepository
    {
        private readonly IMemoryCache _memoryCache;

        public DefaultRepository(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public async Task<T> GetAsync<T>(object aggregateRootId) where T : class, IAggregateRoot
        {
            return await GetAsync(typeof(T), aggregateRootId).ConfigureAwait(false) as T;
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
            var aggregateRoot = await _memoryCache.GetAsync(aggregateRootId, aggregateRootType).ConfigureAwait(false);
            if (aggregateRoot == null)
            {
                aggregateRoot = await _memoryCache.RefreshAggregateFromEventStoreAsync(aggregateRootType, aggregateRootId.ToString()).ConfigureAwait(false);
            }
            return aggregateRoot;
        }
    }
}
