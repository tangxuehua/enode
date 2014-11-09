using System;
using System.Collections.Concurrent;

namespace ENode.Domain.Impl
{
    public class DefaultMemoryCache : IMemoryCache
    {
        private readonly IAggregateRootSerializer _serializer;
        private readonly ConcurrentDictionary<string, byte[]> _aggregateRootDict = new ConcurrentDictionary<string, byte[]>();

        public DefaultMemoryCache(IAggregateRootSerializer serializer)
        {
            _serializer = serializer;
        }

        public IAggregateRoot Get(object aggregateRootId, Type aggregateRootType)
        {
            if (aggregateRootId == null) throw new ArgumentNullException("aggregateRootId");
            byte[] data;
            if (_aggregateRootDict.TryGetValue(aggregateRootId.ToString(), out data))
            {
                return _serializer.Deserialize(data, aggregateRootType);
            }
            return null;
        }
        public T Get<T>(object aggregateRootId) where T : class, IAggregateRoot
        {
            return Get(aggregateRootId, typeof(T)) as T;
        }
        public void Set(IAggregateRoot aggregateRoot)
        {
            if (aggregateRoot == null)
            {
                throw new ArgumentNullException("aggregateRoot");
            }
            _aggregateRootDict[aggregateRoot.UniqueId] = _serializer.Serialize(aggregateRoot);
        }
    }
}
