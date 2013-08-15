using ENode.Domain;

namespace ENode.Snapshoting
{
    /// <summary>An policy interface which used to determine whether should create a snapshot for the aggregate.
    /// </summary>
    public interface ISnapshotPolicy
    {
        /// <summary>Determines whether should create a snapshot for the given aggregate root.
        /// </summary>
        bool ShouldCreateSnapshot(AggregateRoot aggregateRoot);
    }
}
