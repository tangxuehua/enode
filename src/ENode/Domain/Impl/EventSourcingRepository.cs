using System;

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
        /// <param name="id"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>(object id) where T : class, IAggregateRoot
        {
            return Get(typeof(T), id) as T;
        }
        /// <summary>Get an aggregate from memory cache, if not exist, get it from event store.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public IAggregateRoot Get(Type type, object id)
        {
            if (id == null) throw new ArgumentNullException("id");
            return _memoryCache.Get(id, type) ?? _aggregateRootStorage.Get(type, id);
        }
    }
}
