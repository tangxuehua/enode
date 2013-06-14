using System;
using ENode.Domain;

namespace ENode.Snapshoting
{
    public class NoSnapshotPolicy : ISnapshotPolicy
    {
        public bool ShouldCreateSnapshot(AggregateRoot aggregateRoot)
        {
            return false;
        }
    }
}
