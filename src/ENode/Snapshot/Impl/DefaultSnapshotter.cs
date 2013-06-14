using System;
using System.Linq;
using System.Reflection;
using ENode.Domain;

namespace ENode.Snapshoting
{
    public class DefaultSnapshotter : ISnapshotter
    {
        #region Private Variables

        private IAggregateRootFactory _aggregateRootFactory;
        private IAggregateRootTypeProvider _aggregateRootTypeProvider;

        #endregion

        #region Constructors

        public DefaultSnapshotter(IAggregateRootFactory aggregateRootFactory, IAggregateRootTypeProvider aggregateRootTypeProvider)
        {
            _aggregateRootFactory = aggregateRootFactory;
            _aggregateRootTypeProvider = aggregateRootTypeProvider;
        }

        #endregion

        public Snapshot CreateSnapshot(AggregateRoot aggregateRoot)
        {
            if (aggregateRoot == null)
            {
                throw new ArgumentNullException("aggregateRoot");
            }

            if (!IsSnapshotable(aggregateRoot))
            {
                throw new InvalidOperationException(string.Format("聚合根({0})没有实现ISnapshotable接口或者实现了多余1个的ISnapshotable接口，不能对其创建快照。", aggregateRoot.GetType().FullName));
            }

            var snapshotDataType = GetSnapshotDataType(aggregateRoot);
            var snapshotData = SnapshotterHelper.CreateSnapshot(snapshotDataType, aggregateRoot);
            var aggregateRootName = _aggregateRootTypeProvider.GetAggregateRootTypeName(aggregateRoot.GetType());

            return new Snapshot(aggregateRootName, aggregateRoot.UniqueId, aggregateRoot.Version, snapshotData, DateTime.UtcNow);
        }
        public AggregateRoot RestoreFromSnapshot(Snapshot snapshot)
        {
            if (snapshot == null)
            {
                return null;
            }

            var aggregateRootType = _aggregateRootTypeProvider.GetAggregateRootType(snapshot.AggregateRootName);
            var aggregateRoot = _aggregateRootFactory.CreateAggregateRoot(aggregateRootType);
            if (!IsSnapshotable(aggregateRoot))
            {
                throw new InvalidOperationException(string.Format("聚合根({0})没有实现ISnapshotable接口或者实现了多余1个的ISnapshotable接口，不能将其从某个快照还原。", aggregateRoot.GetType().FullName));
            }

            if (GetSnapshotDataType(aggregateRoot) != snapshot.Payload.GetType())
            {
                throw new InvalidOperationException(string.Format("当前聚合根的快照类型({0})与要还原的快照类型({1})不符", GetSnapshotDataType(aggregateRoot), snapshot.Payload.GetType()));
            }

            aggregateRoot.InitializeFromSnapshot(snapshot);

            SnapshotterHelper.RestoreFromSnapshot(snapshot.Payload, aggregateRoot);

            return aggregateRoot;
        }

        private bool IsSnapshotable(AggregateRoot aggregateRoot)
        {
            return aggregateRoot.GetType().GetInterfaces().Count(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ISnapshotable<>)) == 1;
        }
        private Type GetSnapshotDataType(AggregateRoot aggregateRoot)
        {
            return aggregateRoot.GetType().GetInterfaces().Single(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ISnapshotable<>)).GetGenericArguments()[0];
        }

        class SnapshotterHelper
        {
            private static MethodInfo _createSnapshotMethod = typeof(SnapshotterHelper).GetMethod("CreateSnapshot", BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.NonPublic);
            private static MethodInfo _restoreFromSnapshotMethod = typeof(SnapshotterHelper).GetMethod("RestoreFromSnapshot", BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.NonPublic);
            private static SnapshotterHelper _instance = new SnapshotterHelper();

            public static object CreateSnapshot(Type snapshotDataType, AggregateRoot aggregateRoot)
            {
                return _createSnapshotMethod.MakeGenericMethod(snapshotDataType).Invoke(_instance, new object[] { aggregateRoot });
            }
            public static object RestoreFromSnapshot(object snapshotData, AggregateRoot aggregateRoot)
            {
                return _restoreFromSnapshotMethod.MakeGenericMethod(snapshotData.GetType()).Invoke(_instance, new object[] { aggregateRoot, snapshotData });
            }

            private TSnapshot CreateSnapshot<TSnapshot>(AggregateRoot aggregateRoot)
            {
                return ((ISnapshotable<TSnapshot>)aggregateRoot).CreateSnapshot();
            }
            private void RestoreFromSnapshot<TSnapshot>(AggregateRoot aggregateRoot, TSnapshot snapshot)
            {
                ((ISnapshotable<TSnapshot>)aggregateRoot).RestoreFromSnapshot(snapshot);
            }
        }
    }
}
