using ENode.Domain;

namespace ENode.Redis
{
    /// <summary>ENode configuration class Redis extensions.
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>Use Redis as the memory cache for the enode framework.
        /// </summary>
        /// <returns></returns>
        public static Configuration UseRedis(this Configuration configuration)
        {
            return UseRedis(configuration, "127.0.0.1", 6379);
        }
        /// <summary>Use Redis as the memory cache for the enode framework.
        /// </summary>
        /// <returns></returns>
        public static Configuration UseRedis(this Configuration configuration, string host, int port)
        {
            configuration.SetDefault<IMemoryCache, RedisMemoryCache>(new RedisMemoryCache(host, port));
            return configuration;
        }
    }
}