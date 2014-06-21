using System;
using ENode.Commanding;
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
        /// <exception cref="ArgumentNullException">Throwed when the aggregateRootType or aggregateRootId is null.</exception>
        /// <exception cref="AggregateRootNotExistException">Throwed when the aggregate root not found.</exception>
        /// <exception cref="ENodeException">Throwed when calling the memory cache has exception.</exception>
        public T Get<T>(object aggregateRootId) where T : class, IAggregateRoot
        {
            return Get(typeof(T), aggregateRootId) as T;
        }
        /// <summary>Get an aggregate from memory cache, if not exist, get it from event store.
        /// </summary>
        /// <param name="aggregateRootType"></param>
        /// <param name="aggregateRootId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Throwed when the aggregateRootType or aggregateRootId is null.</exception>
        /// <exception cref="AggregateRootNotExistException">Throwed when the aggregate root not found.</exception>
        /// <exception cref="ENodeException">Throwed when calling the memory cache has exception.</exception>
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
