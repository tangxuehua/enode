using ENode.Domain;

namespace ENode.Snapshoting
{
    /// <summary>An interface which can create snapshot for aggregate or restore aggregate from snapshot.
    /// </summary>
    public interface ISnapshotter
    {
        /// <summary>Create a snapshot for the given aggregate root.
        /// </summary>
        Snapshot CreateSnapshot(AggregateRoot aggregateRoot);
        /// <summary>Restore the aggregate from the given snapshot.
        /// </summary>
        AggregateRoot RestoreFromSnapshot(Snapshot snapshot);
    }
}
