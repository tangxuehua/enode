using System;
using System.Threading.Tasks;

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
        Task<IAggregateRoot> GetAsync(Type aggregateRootType, string aggregateRootId);
    }
}
