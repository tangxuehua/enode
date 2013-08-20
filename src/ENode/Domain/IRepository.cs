using System;

namespace ENode.Domain
{
    /// <summary>Represents a repository of the building block of Eric Evans's DDD.
    /// </summary>
    public interface IRepository
    {
        /// <summary>Get a aggregate from event store.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        T Get<T>(string id) where T : AggregateRoot;
        /// <summary>Get a aggregate from event store.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        AggregateRoot Get(Type type, string id);
    }
}
