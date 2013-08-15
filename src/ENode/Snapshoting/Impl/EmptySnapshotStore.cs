using System;

namespace ENode.Snapshoting.Impl
{
    /// <summary>Represents a snapshot store that always not store any snapshot.
    /// </summary>
    public class EmptySnapshotStore : ISnapshotStore
    {
        /// <summary>Do nothing.
        /// </summary>
        /// <param name="snapshot"></param>
        public void StoreShapshot(Snapshot snapshot)
        {
        }
        /// <summary>Always return null.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        /// <param name="aggregateRootType"></param>
        /// <returns></returns>
        public Snapshot GetLastestSnapshot(string aggregateRootId, Type aggregateRootType)
        {
            return null;
        }
    }
}
