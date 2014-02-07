using System;

namespace ENode.Domain
{
    /// <summary>Represents an aggregate storage interface.
    /// </summary>
    public interface IAggregateStorage
    {
        /// <summary>Get an aggregate from aggregate storage.
        /// </summary>
        /// <param name="aggregateRootType"></param>
        /// <param name="aggregateRootId"></param>
        /// <returns></returns>
        IAggregateRoot Get(Type aggregateRootType, string aggregateRootId);
    }
}
