using ENode.Domain;

namespace ENode.Snapshoting.Impl
{
    /// <summary>A policy that always not create snapshot.
    /// </summary>
    public class NoSnapshotPolicy : ISnapshotPolicy
    {
        /// <summary>Always return false.
        /// </summary>
        /// <param name="aggregateRoot"></param>
        /// <returns></returns>
        public bool ShouldCreateSnapshot(AggregateRoot aggregateRoot)
        {
            return false;
        }
    }
}
