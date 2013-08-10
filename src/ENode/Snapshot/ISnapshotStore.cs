using System;

namespace ENode.Snapshoting {
    /// <summary>定义用于存储聚合根快照的接口
    /// </summary>
    public interface ISnapshotStore {
        /// <summary>持久化给定的聚合根快照
        /// </summary>
        /// <param name="snapshot">要持久化的聚合根快照</param>
        void StoreShapshot(Snapshot snapshot);
        /// <summary>获取指定聚合根最新生成的一个快照
        /// </summary>
        /// <param name="aggregateRootId">聚合根ID</param>
        /// <param name="aggregateRootType">聚合根类型</param>
        /// <returns>返回最新生成的一个快照，如果存在的话</returns>
        Snapshot GetLastestSnapshot(string aggregateRootId, Type aggregateRootType);
    }
}
