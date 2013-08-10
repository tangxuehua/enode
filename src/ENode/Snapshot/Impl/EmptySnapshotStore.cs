using System;

namespace ENode.Snapshoting {
    public class EmptySnapshotStore : ISnapshotStore {
        public void StoreShapshot(Snapshot snapshot) {
        }
        public Snapshot GetLastestSnapshot(string aggregateRootId, Type aggregateRootType) {
            return null;
        }
    }
}
