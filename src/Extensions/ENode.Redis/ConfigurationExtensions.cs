using ENode.Configurations;
using ENode.Domain;

namespace ENode.Redis
{
    /// <summary>ENode configuration class Redis extensions.
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>Use Redis to implement the memory cache.
        /// </summary>
        /// <returns></returns>
        public static ENodeConfiguration UseRedisMemoryCache(this ENodeConfiguration configuration)
        {
            return UseRedisMemoryCache(configuration, "127.0.0.1", 6379);
        }
        /// <summary>Use Redis to implement the memory cache.
        /// </summary>
        /// <returns></returns>
        public static ENodeConfiguration UseRedisMemoryCache(this ENodeConfiguration configuration, string host, int port)
        {
            configuration.GetCommonConfiguration().SetDefault<IMemoryCache, RedisMemoryCache>(new RedisMemoryCache(host, port));
            return configuration;
        }
    }
}