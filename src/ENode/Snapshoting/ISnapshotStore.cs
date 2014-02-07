using System;

namespace ENode.Snapshoting
{
    /// <summary>An interface to store the snapshot.
    /// </summary>
    public interface ISnapshotStore
    {
        /// <summary>Store the given snapshot.
        /// </summary>
        /// <param name="snapshot">The snapshot to store.</param>
        void StoreShapshot(Snapshot snapshot);
        /// <summary>Get the latest snapshot for the specified aggregate root.
        /// </summary>
        /// <param name="aggregateRootId">The aggregate root id.</param>
        /// <param name="aggregateRootType">The aggregate root type.</param>
        /// <returns>Returns the snapshot if exist; otherwise, returns null.</returns>
        Snapshot GetLastestSnapshot(string aggregateRootId, Type aggregateRootType);
    }
}
