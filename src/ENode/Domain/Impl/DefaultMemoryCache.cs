using System;
using System.Collections.Concurrent;
using ECommon.Serializing;

namespace ENode.Domain.Impl
{
    /// <summary>Default implementation of IMemoryCache which using ConcurrentDictionary.
    /// </summary>
    public class DefaultMemoryCache : IMemoryCache
    {
        private readonly ConcurrentDictionary<string, byte[]> _cacheDict = new ConcurrentDictionary<string, byte[]>();
        private readonly IBinarySerializer _binarySerializer;
        private readonly IEventSourcingService _eventSourcingService;

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="binarySerializer"></param>
        /// <param name="eventSourcingService"></param>
        public DefaultMemoryCache(IBinarySerializer binarySerializer, IEventSourcingService eventSourcingService)
        {
            _binarySerializer = binarySerializer;
            _eventSourcingService = eventSourcingService;
        }

        /// <summary>Get an aggregate from memory cache.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        /// <param name="aggregateRootType"></param>
        /// <returns></returns>
        public IAggregateRoot Get(object aggregateRootId, Type aggregateRootType)
        {
            if (aggregateRootId == null) throw new ArgumentNullException("aggregateRootId");
            byte[] value;
            if (_cacheDict.TryGetValue(aggregateRootId.ToString(), out value))
            {
                var aggregateRoot = _binarySerializer.Deserialize(value, aggregateRootType) as IAggregateRoot;
                _eventSourcingService.InitializeAggregateRoot(aggregateRoot);
                return aggregateRoot;
            }
            return null;
        }
        /// <summary>Get a strong type aggregate from memory cache.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="aggregateRootId"></param>
        /// <returns></returns>
        public T Get<T>(object aggregateRootId) where T : class, IAggregateRoot
        {
            return Get(aggregateRootId, typeof(T)) as T;
        }
        /// <summary>Set an aggregate to memory cache.
        /// </summary>
        /// <param name="aggregateRoot"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void Set(IAggregateRoot aggregateRoot)
        {
            if (aggregateRoot == null)
            {
                throw new ArgumentNullException("aggregateRoot");
            }
            _cacheDict[aggregateRoot.UniqueId] = _binarySerializer.Serialize(aggregateRoot);
        }
    }
}
