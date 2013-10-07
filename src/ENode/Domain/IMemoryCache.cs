using System;
namespace ENode.Domain
{
    /// <summary>Represents a high speed memory cache to get or set aggregate.
    /// </summary>
    public interface IMemoryCache
    {
        /// <summary>Get an aggregate from memory cache.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        AggregateRoot Get(object id, Type type);
        /// <summary>Get a strong type aggregate from memory cache.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        T Get<T>(object id) where T : AggregateRoot;
        /// <summary>Set an aggregate to memory cache.
        /// </summary>
        /// <param name="aggregateRoot"></param>
        void Set(AggregateRoot aggregateRoot);
    }
}
