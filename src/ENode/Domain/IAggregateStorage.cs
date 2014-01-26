using System;

namespace ENode.Domain
{
    /// <summary>Represents an aggregate storage interface.
    /// </summary>
    public interface IAggregateStorage
    {
        /// <summary>Get an aggregate from event store.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        IAggregateRoot Get(Type type, object id);
    }
}
