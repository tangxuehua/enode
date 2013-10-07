using System;
using ENode.Domain;
using ENode.Infrastructure;
using ENode.Infrastructure.Serializing;
using ServiceStack.Redis;

namespace ENode.Redis
{
    /// <summary>Redis based memory cache implementation.
    /// </summary>
    public class RedisMemoryCache : IMemoryCache
    {
        private readonly RedisClient _redisClient;
        private readonly IBinarySerializer _binarySerializer;

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        public RedisMemoryCache(string host, int port)
        {
            _redisClient = new RedisClient(host, port);
            _binarySerializer = ObjectContainer.Resolve<IBinarySerializer>();
        }

        /// <summary>Get an aggregate from memory cache.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public AggregateRoot Get(object id, Type type)
        {
            if (id == null) throw new ArgumentNullException("id");
            var value = _redisClient.Get(id.ToString());
            if (value != null && value.Length > 0)
            {
                return _binarySerializer.Deserialize(value, type) as AggregateRoot;
            }
            return null;
        }
        /// <summary>Get a strong type aggregate from memory cache.
        /// </summary>
        /// <param name="id"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>(object id) where T : AggregateRoot
        {
            if (id == null) throw new ArgumentNullException("id");
            var value = _redisClient.Get(id.ToString());
            if (value != null && value.Length > 0)
            {
                return _binarySerializer.Deserialize<T>(value);
            }
            return null;
        }
        /// <summary>Set an aggregate to memory cache.
        /// </summary>
        /// <param name="aggregateRoot"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void Set(AggregateRoot aggregateRoot)
        {
            if (aggregateRoot == null)
            {
                throw new ArgumentNullException("aggregateRoot");
            }
            _redisClient.Set(aggregateRoot.UniqueId, _binarySerializer.Serialize(aggregateRoot));
        }
    }
}
