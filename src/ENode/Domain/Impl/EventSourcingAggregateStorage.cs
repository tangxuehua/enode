using System;
using System.Collections.Generic;
using System.Linq;
using ENode.Eventing;
using ENode.Infrastructure;
using ENode.Snapshoting;

namespace ENode.Domain.Impl
{
    public class EventSourcingAggregateStorage : IAggregateStorage
    {
        private const int minVersion = 1;
        private const int maxVersion = int.MaxValue;
        private readonly IAggregateRootFactory _aggregateRootFactory;
        private readonly IEventStore _eventStore;
        private readonly ISnapshotStore _snapshotStore;
        private readonly ISnapshotter _snapshotter;
        private readonly ITypeNameProvider _typeNameProvider;

        public EventSourcingAggregateStorage(
            IAggregateRootFactory aggregateRootFactory,
            IEventStore eventStore,
            ISnapshotStore snapshotStore,
            ISnapshotter snapshotter,
            ITypeNameProvider typeNameProvider)
        {
            _aggregateRootFactory = aggregateRootFactory;
            _eventStore = eventStore;
            _snapshotStore = snapshotStore;
            _snapshotter = snapshotter;
            _typeNameProvider = typeNameProvider;
        }

        public IAggregateRoot Get(Type aggregateRootType, string aggregateRootId)
        {
            if (aggregateRootId == null) throw new ArgumentNullException("aggregateRootId");

            var aggregateRoot = default(IAggregateRoot);

            if (TryGetFromSnapshot(aggregateRootId, aggregateRootType, out aggregateRoot))
            {
                return aggregateRoot;
            }

            var aggregateRootTypeName = _typeNameProvider.GetTypeName(aggregateRootType);
            var eventStreams = _eventStore.QueryAggregateEvents(aggregateRootId, aggregateRootTypeName, minVersion, maxVersion);
            aggregateRoot = RebuildAggregateRoot(aggregateRootType, eventStreams);

            return aggregateRoot;
        }

        #region Helper Methods

        private bool TryGetFromSnapshot(string aggregateRootId, Type aggregateRootType, out IAggregateRoot aggregateRoot)
        {
            aggregateRoot = null;

            var snapshot = _snapshotStore.GetLastestSnapshot(aggregateRootId, aggregateRootType);
            if (snapshot == null) return false;

            aggregateRoot = _snapshotter.RestoreFromSnapshot(snapshot);
            if (aggregateRoot == null) return false;

            if (aggregateRoot.UniqueId != aggregateRootId)
            {
                throw new Exception(string.Format("Aggregate root restored from snapshot not valid as the aggregate root id not matched. Snapshot aggregate root id:{0}, expected aggregate root id:{1}", aggregateRoot.UniqueId, aggregateRootId));
            }

            var aggregateRootTypeName = _typeNameProvider.GetTypeName(aggregateRootType);
            var eventStreamsAfterSnapshot = _eventStore.QueryAggregateEvents(aggregateRootId, aggregateRootTypeName, snapshot.Version + 1, int.MaxValue);
            aggregateRoot.ReplayEvents(eventStreamsAfterSnapshot);
            return true;
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
