using System;
using System.Collections.Concurrent;
using ECommon.Serializing;

namespace ENode.Domain.Impl
{
    /// <summary>Default implementation of IMemoryCache which using ConcurrentDictionary.
    /// </summary>
    public class DefaultMemoryCache : IMemoryCache
    {
        private readonly ConcurrentDictionary<string, IAggregateRoot> _cacheDict = new ConcurrentDictionary<string, IAggregateRoot>();

        /// <summary>Get an aggregate from memory cache.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        /// <param name="aggregateRootType"></param>
        /// <returns></returns>
        public IAggregateRoot Get(object aggregateRootId, Type aggregateRootType)
        {
            if (aggregateRootId == null) throw new ArgumentNullException("aggregateRootId");
            IAggregateRoot aggregateRoot;
            if (_cacheDict.TryGetValue(aggregateRootId.ToString(), out aggregateRoot))
            {
                if (aggregateRoot.GetType() != aggregateRootType)
                {
                    throw new Exception(string.Format("Incorrect aggregate root type, current aggregateRootId:{0}, type:{1}, expecting type:{2}", aggregateRootId, aggregateRoot.GetType(), aggregateRootType));
                }
                aggregateRoot.ResetChanges();
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
            _cacheDict[aggregateRoot.UniqueId] = aggregateRoot;
        }
    }
}
