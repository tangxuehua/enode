using System;

namespace ENode.Snapshoting {
    /// <summary>定义一个接口，如果某个聚合根实现该接口，表明该聚合根支持快照
    /// </summary>
    public interface ISnapshotable<TSnapshot> {
        TSnapshot CreateSnapshot();
        void RestoreFromSnapshot(TSnapshot snapshot);
    }
}
