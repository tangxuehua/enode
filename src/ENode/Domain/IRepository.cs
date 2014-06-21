using System;

namespace ENode.Domain
{
    /// <summary>Represents a repository of the building block of Eric Evans's DDD.
    /// </summary>
    public interface IRepository
    {
        /// <summary>Get an aggregate from memory cache, if not exist, get it from event store.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Throwed when the aggregateRootType or aggregateRootId is null.</exception>
        /// <exception cref="AggregateRootNotExistException">Throwed when the aggregate root not found.</exception>
        /// <exception cref="ENodeException">Throwed when calling the memory cache has exception.</exception>
        T Get<T>(object aggregateRootId) where T : class, IAggregateRoot;
        /// <summary>Get an aggregate from memory cache, if not exist, get it from event store.
        /// </summary>
        /// <param name="aggregateRootType"></param>
        /// <param name="aggregateRootId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Throwed when the aggregateRootType or aggregateRootId is null.</exception>
        /// <exception cref="AggregateRootNotExistException">Throwed when the aggregate root not found.</exception>
        /// <exception cref="ENodeException">Throwed when calling the memory cache has exception.</exception>
        IAggregateRoot Get(Type aggregateRootType, object aggregateRootId);
    }
}
