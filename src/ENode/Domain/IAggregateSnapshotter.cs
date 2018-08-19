using System;
using System.Threading.Tasks;
using ENode.Domain;

namespace ENode.Domain
{
    /// <summary>An interface which can restore aggregate from snapshot storage.
    /// </summary>
    public interface IAggregateSnapshotter
    {
        /// <summary>Restore the aggregate from snapshot storage.
        /// </summary>
        Task<IAggregateRoot> RestoreFromSnapshotAsync(Type aggregateRootType, string aggregateRootId);
    }
}
