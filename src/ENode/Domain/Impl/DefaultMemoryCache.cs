using System;
using System.Collections.Concurrent;
using ENode.Infrastructure.Serializing;

namespace ENode.Domain.Impl
{
    public class DefaultMemoryCache : IMemoryCache
    {
        private ConcurrentDictionary<string, byte[]> _cacheDict = new ConcurrentDictionary<string, byte[]>();
        private IBinarySerializer _binarySerializer;

        public DefaultMemoryCache(IBinarySerializer binarySerializer)
        {
            _binarySerializer = binarySerializer;
        }

        public AggregateRoot Get(string id)
        {
            byte[] value;
            if (_cacheDict.TryGetValue(id, out value))
            {
                return _binarySerializer.Deserialize(value) as AggregateRoot;
            }
            return null;
        }
        public T Get<T>(string id) where T : AggregateRoot
        {
            byte[] value;
            if (_cacheDict.TryGetValue(id, out value))
            {
                return _binarySerializer.Deserialize<T>(value);
            }
            return null;
        }
        public void Set(AggregateRoot aggregateRoot)
        {
            if (aggregateRoot == null)
            {
                throw new ArgumentNullException("aggregateRoot");
            }
            _cacheDict[aggregateRoot.UniqueId] = _binarySerializer.Serialize(aggregateRoot);
        }
    }
}
