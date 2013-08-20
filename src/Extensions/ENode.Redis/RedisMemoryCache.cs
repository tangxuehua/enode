using System;
using ENode.Domain;
using ENode.Infrastructure;
using ENode.Infrastructure.Serializing;
using ServiceStack.Redis;

namespace ENode.Redis
{
    /// <summary>
    /// 
    /// </summary>
    public class RedisMemoryCache : IMemoryCache
    {
        private readonly RedisClient _redisClient;
        private readonly IBinarySerializer _binarySerializer;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        public RedisMemoryCache(string host, int port)
        {
            _redisClient = new RedisClient(host, port);
            _binarySerializer = ObjectContainer.Resolve<IBinarySerializer>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public AggregateRoot Get(string id)
        {
            var value = _redisClient.Get(id);
            if (value != null && value.Length > 0)
            {
                return _binarySerializer.Deserialize(value) as AggregateRoot;
            }
            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>(string id) where T : AggregateRoot
        {
            var value = _redisClient.Get(id);
            if (value != null && value.Length > 0)
            {
                return _binarySerializer.Deserialize<T>(value);
            }
            return null;
        }
        /// <summary>
        /// 
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
