using ENode.Domain;

namespace ENode.Snapshoting.Impl
{
    public class NoSnapshotPolicy : ISnapshotPolicy
    {
        public bool ShouldCreateSnapshot(IAggregateRoot aggregateRoot)
        {
            return false;
        }
    }
}
