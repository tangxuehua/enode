using System;
using ENode.Domain;

namespace ENode.Snapshoting
{
    /// <summary>快照处理接口定义
    /// </summary>
    public interface ISnapshotter
    {
        /// <summary>为给定的聚合根创建快照
        /// </summary>
        Snapshot CreateSnapshot(AggregateRoot aggregateRoot);
        /// <summary>从给定的快照还原聚合根
        /// </summary>
        AggregateRoot RestoreFromSnapshot(Snapshot snapshot);
    }
}
