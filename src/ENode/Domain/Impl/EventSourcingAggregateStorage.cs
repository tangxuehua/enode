using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ECommon.IO;
using ENode.Eventing;
using ENode.Infrastructure;

namespace ENode.Domain.Impl
{
    public class EventSourcingAggregateStorage : IAggregateStorage
    {
        private const int minVersion = 1;
        private const int maxVersion = int.MaxValue;
        private readonly IAggregateRootFactory _aggregateRootFactory;
        private readonly IEventStore _eventStore;
        private readonly IAggregateSnapshotter _aggregateSnapshotter;
        private readonly ITypeNameProvider _typeNameProvider;
        private readonly IOHelper _ioHelper;

        public EventSourcingAggregateStorage(
            IAggregateRootFactory aggregateRootFactory,
            IEventStore eventStore,
            IAggregateSnapshotter aggregateSnapshotter,
            ITypeNameProvider typeNameProvider,
            IOHelper ioHelper)
        {
            _aggregateRootFactory = aggregateRootFactory;
            _eventStore = eventStore;
            _aggregateSnapshotter = aggregateSnapshotter;
            _typeNameProvider = typeNameProvider;
            _ioHelper = ioHelper;
        }

        public async Task<IAggregateRoot> GetAsync(Type aggregateRootType, string aggregateRootId)
        {
            if (aggregateRootType == null) throw new ArgumentNullException("aggregateRootType");
            if (aggregateRootId == null) throw new ArgumentNullException("aggregateRootId");

            var aggregateRoot = await TryGetFromSnapshot(aggregateRootId, aggregateRootType).ConfigureAwait(false);
            if (aggregateRoot != null)
            {
                return aggregateRoot;
            }

            var aggregateRootTypeName = _typeNameProvider.GetTypeName(aggregateRootType);

            var eventStreams = await TryQueryAggregateEventsAsync(aggregateRootType, aggregateRootTypeName, aggregateRootId, minVersion, maxVersion, 0, new TaskCompletionSource<IEnumerable<DomainEventStream>>()).ConfigureAwait(false);

            return RebuildAggregateRoot(aggregateRootType, eventStreams);
        }

        #region Helper Methods

        private async Task<IAggregateRoot> TryGetFromSnapshot(string aggregateRootId, Type aggregateRootType)
        {
            var aggregateRoot = await TryRestoreFromSnapshotAsync(aggregateRootType, aggregateRootId, 0, new TaskCompletionSource<IAggregateRoot>()).ConfigureAwait(false);
            if (aggregateRoot == null)
            {
                return null;
            }

            if (aggregateRoot.GetType() != aggregateRootType || aggregateRoot.UniqueId != aggregateRootId)
            {
                throw new Exception(string.Format("AggregateRoot recovery from snapshot is invalid as the aggregateRootType or aggregateRootId is not matched. Snapshot: [aggregateRootType:{0},aggregateRootId:{1}], expected: [aggregateRootType:{2},aggregateRootId:{3}]",
                    aggregateRoot.GetType(),
                    aggregateRoot.UniqueId,
                    aggregateRootType,
                    aggregateRootId));
            }

            var aggregateRootTypeName = _typeNameProvider.GetTypeName(aggregateRootType);
            var eventStreams = await TryQueryAggregateEventsAsync(aggregateRootType, aggregateRootTypeName, aggregateRootId, aggregateRoot.Version + 1, maxVersion, 0, new TaskCompletionSource<IEnumerable<DomainEventStream>>()).ConfigureAwait(false);
            aggregateRoot.ReplayEvents(eventStreams);
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
        private Task<IEnumerable<DomainEventStream>> TryQueryAggregateEventsAsync(Type aggregateRootType, string aggregateRootTypeName, string aggregateRootId, int minVersion, int maxVersion, int retryTimes, TaskCompletionSource<IEnumerable<DomainEventStream>> taskSource)
        {
            _ioHelper.TryAsyncActionRecursively("TryQueryAggregateEventsAsync",
            () => _eventStore.QueryAggregateEventsAsync(aggregateRootId, aggregateRootTypeName, minVersion, maxVersion),
            currentRetryTimes => TryQueryAggregateEventsAsync(aggregateRootType, aggregateRootTypeName, aggregateRootId, minVersion, maxVersion, currentRetryTimes, taskSource),
            result =>
            {
                taskSource.SetResult(result);
            },
            () => string.Format("_eventStore.QueryAggregateEventsAsync has unknown exception, aggregateRootTypeName: {0}, aggregateRootId: {1}", aggregateRootTypeName, aggregateRootId),
            null,
            retryTimes, true);
            return taskSource.Task;
        }
        private IAggregateRoot RebuildAggregateRoot(Type aggregateRootType, IEnumerable<DomainEventStream> eventStreams)
        {
            if (eventStreams == null || !eventStreams.Any()) return null;

            var aggregateRoot = _aggregateRootFactory.CreateAggregateRoot(aggregateRootType);
            aggregateRoot.ReplayEvents(eventStreams);

            return aggregateRoot;
        }

        #endregion
    }
}
