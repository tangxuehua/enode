using System;
using System.Threading.Tasks;
using ECommon.IO;

namespace ENode.Domain.Impl
{
    public class SnapshotOnlyAggregateStorage : IAggregateStorage
    {
        private readonly IAggregateSnapshotter _aggregateSnapshotter;
        private readonly IOHelper _ioHelper;

        public SnapshotOnlyAggregateStorage(IAggregateSnapshotter aggregateSnapshotter, IOHelper ioHelper)
        {
            _aggregateSnapshotter = aggregateSnapshotter;
            _ioHelper = ioHelper;
        }

        public async Task<IAggregateRoot> GetAsync(Type aggregateRootType, string aggregateRootId)
        {
            if (aggregateRootType == null) throw new ArgumentNullException("aggregateRootType");
            if (aggregateRootId == null) throw new ArgumentNullException("aggregateRootId");

            var aggregateRoot = await TryRestoreFromSnapshotAsync(aggregateRootType, aggregateRootId, 0, new TaskCompletionSource<IAggregateRoot>()).ConfigureAwait(false);
            if (aggregateRoot != null && (aggregateRoot.GetType() != aggregateRootType || aggregateRoot.UniqueId != aggregateRootId))
            {
                throw new Exception(string.Format("AggregateRoot recovery from snapshot is invalid as the aggregateRootType or aggregateRootId is not matched. Snapshot: [aggregateRootType:{0},aggregateRootId:{1}], expected: [aggregateRootType:{2},aggregateRootId:{3}]",
                    aggregateRoot.GetType(),
                    aggregateRoot.UniqueId,
                    aggregateRootType,
                    aggregateRootId));
            }

            return aggregateRoot;
        }

        private Task<IAggregateRoot> TryRestoreFromSnapshotAsync(Type aggregateRootType, string aggregateRootId, int retryTimes, TaskCompletionSource<IAggregateRoot> taskSource)
        {
            _ioHelper.TryAsyncActionRecursively("TryRestoreFromSnapshotAsync",
            () => _aggregateSnapshotter.RestoreFromSnapshotAsync(aggregateRootType, aggregateRootId),
            currentRetryTimes => TryRestoreFromSnapshotAsync(aggregateRootType, aggregateRootId, currentRetryTimes, taskSource),
            result =>
            {
                taskSource.SetResult(result);
            },
            () => string.Format("_aggregateSnapshotter.TryRestoreFromSnapshotAsync has unknown exception, aggregateRootType: {0}, aggregateRootId: {1}", aggregateRootType.FullName, aggregateRootId),
            null,
            retryTimes, true);
            return taskSource.Task;
        }
    }
}
