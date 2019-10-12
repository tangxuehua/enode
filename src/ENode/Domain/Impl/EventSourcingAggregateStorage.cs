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

        public EventSourcingAggregateStorage(
            IAggregateRootFactory aggregateRootFactory,
            IEventStore eventStore,
            IAggregateSnapshotter aggregateSnapshotter,
            ITypeNameProvider typeNameProvider)
        {
            _aggregateRootFactory = aggregateRootFactory;
            _eventStore = eventStore;
            _aggregateSnapshotter = aggregateSnapshotter;
            _typeNameProvider = typeNameProvider;
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
            var eventStreams = await _eventStore.QueryAggregateEventsAsync(aggregateRootId, aggregateRootTypeName, minVersion, maxVersion).ConfigureAwait(false);
            aggregateRoot = RebuildAggregateRoot(aggregateRootType, eventStreams);
            return aggregateRoot;
        }

        #region Helper Methods

        private async Task<IAggregateRoot> TryGetFromSnapshot(string aggregateRootId, Type aggregateRootType)
        {
            var aggregateRoot = await _aggregateSnapshotter.RestoreFromSnapshotAsync(aggregateRootType, aggregateRootId).ConfigureAwait(false);

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
            var eventStreams = await _eventStore.QueryAggregateEventsAsync(aggregateRootId, aggregateRootTypeName, aggregateRoot.Version + 1, int.MaxValue).ConfigureAwait(false);
            aggregateRoot.ReplayEvents(eventStreams);
            return aggregateRoot;
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
