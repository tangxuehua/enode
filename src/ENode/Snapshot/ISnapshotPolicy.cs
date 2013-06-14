using System;
using ENode.Domain;

namespace ENode.Snapshoting
{
    /// <summary>聚合根快照创建策略接口，此接口用于决定是否应该对当前聚合根创建快照
    /// </summary>
    public interface ISnapshotPolicy
    {
        /// <summary>根据某种策略判断是否应该对当前聚合根创建快照
        /// </summary>
        bool ShouldCreateSnapshot(AggregateRoot aggregateRoot);
    }
}
