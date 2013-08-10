using System;
using ENode.Domain;
using ENode.Infrastructure;
using ServiceStack.Redis;

namespace ENode.Redis {
    public class RedisMemoryCache : IMemoryCache {
        private RedisClient _redisClient;
        private IBinarySerializer _binarySerializer;

        public RedisMemoryCache(string host, int port) {
            _redisClient = new RedisClient(host, port);
            _binarySerializer = ObjectContainer.Resolve<IBinarySerializer>();
        }

        public AggregateRoot Get(string id) {
            byte[] value = _redisClient.Get(id);
            if (value != null && value.Length > 0) {
                return _binarySerializer.Deserialize(value) as AggregateRoot;
            }
            return null;
        }
        public T Get<T>(string id) where T : AggregateRoot {
            byte[] value = _redisClient.Get(id);
            if (value != null && value.Length > 0) {
                return _binarySerializer.Deserialize<T>(value);
            }
            return null;
        }
        public void Set(AggregateRoot aggregateRoot) {
            if (aggregateRoot == null) {
                throw new ArgumentNullException("aggregateRoot");
            }
            _redisClient.Set(aggregateRoot.UniqueId, _binarySerializer.Serialize(aggregateRoot));
        }
    }
}
