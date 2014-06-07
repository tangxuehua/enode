using System;
using ENode.Infrastructure;

namespace ENode.Domain.Impl
{
    /// <summary>An repository implementation with the event sourcing pattern.
    /// </summary>
    public class EventSourcingRepository : IRepository
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IAggregateStorage _aggregateRootStorage;

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="memoryCache"></param>
        /// <param name="aggregateRootStorage"></param>
        public EventSourcingRepository(IMemoryCache memoryCache, IAggregateStorage aggregateRootStorage)
        {
            _memoryCache = memoryCache;
            _aggregateRootStorage = aggregateRootStorage;
        }

        /// <summary>Get an aggregate from memory cache, if not exist, get it from event store.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>(object aggregateRootId) where T : class, IAggregateRoot
        {
            return Get(typeof(T), aggregateRootId) as T;
        }
        /// <summary>Get an aggregate from memory cache, if not exist, get it from event store.
        /// </summary>
        /// <param name="aggregateRootType"></param>
        /// <param name="aggregateRootId"></param>
        /// <returns></returns>
        public IAggregateRoot Get(Type aggregateRootType, object aggregateRootId)
        {
            if (aggregateRootType == null)
            {
                throw new ENodeException("aggregateRootType cannot be null.");
            }
            if (aggregateRootId == null)
            {
                throw new ENodeException("aggregateRootId cannot be null.");
            }
            try
            {
                return _memoryCache.Get(aggregateRootId, aggregateRootType) ?? _aggregateRootStorage.Get(aggregateRootType, aggregateRootId.ToString());
            }
            catch (Exception ex)
            {
                throw new ENodeException(
                    string.Format("Get aggregate from repoisotry has exception, aggregateRootType:{0}, aggregateRootId:{1}", aggregateRootType, aggregateRootId),
                    ex);
            }
        }
    }
}
