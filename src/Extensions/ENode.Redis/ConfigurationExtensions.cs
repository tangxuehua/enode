using ENode.Domain;
using ENode.Eventing;
using ENode.Infrastructure.Serializing;

namespace ENode.Redis
{
    /// <summary>ENode configuration class Redis extensions.
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>Use Redis to implement the memory cache for the enode framework.
        /// </summary>
        /// <returns></returns>
        public static Configuration UseRedisMemoryCache(this Configuration configuration)
        {
            return UseRedisMemoryCache(configuration, "127.0.0.1", 6379);
        }
        /// <summary>Use Redis to implement the memory cache for the enode framework.
        /// </summary>
        /// <returns></returns>
        public static Configuration UseRedisMemoryCache(this Configuration configuration, string host, int port)
        {
            configuration.SetDefault<IMemoryCache, RedisMemoryCache>(new RedisMemoryCache(host, port));
            return configuration;
        }
        /// <summary>Use Redis to implement the eventstore for the enode framework.
        /// </summary>
        /// <returns></returns>
        public static Configuration UseRedisEventStore(this Configuration configuration)
        {
            return UseRedisEventStore(configuration, "127.0.0.1", 6379);
        }
        /// <summary>Use Redis to implement the eventstore for the enode framework.
        /// </summary>
        /// <returns></returns>
        public static Configuration UseRedisEventStore(this Configuration configuration, string host, int port)
        {
            configuration.SetDefault<IEventStore, RedisEventStore>(new RedisEventStore(host, port));
            return configuration;
        }
        /// <summary>Use ServiceStack.Redis to implement the binary serializer for the enode framework.
        /// </summary>
        /// <returns></returns>
        public static Configuration UseRedisBinarySerializer(this Configuration configuration)
        {
            configuration.SetDefault<IBinarySerializer, RedisBinarySerializer>(new RedisBinarySerializer());
            return configuration;
        }
    }
}