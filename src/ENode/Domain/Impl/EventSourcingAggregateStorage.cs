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
        private readonly IEventSourcingService _eventSourcingService;
        private readonly IEventStore _eventStore;
        private readonly ISnapshotStore _snapshotStore;
        private readonly ISnapshotter _snapshotter;
        private readonly ITypeCodeProvider<IAggregateRoot> _aggregateRootTypeCodeProvider;

        public EventSourcingAggregateStorage(
            IAggregateRootFactory aggregateRootFactory,
            IEventSourcingService eventSourcingService,
            IEventStore eventStore,
            ISnapshotStore snapshotStore,
            ISnapshotter snapshotter,
            ITypeCodeProvider<IAggregateRoot> aggregateRootTypeCodeProvider)
        {
            _aggregateRootFactory = aggregateRootFactory;
            _eventSourcingService = eventSourcingService;
            _eventStore = eventStore;
            _snapshotStore = snapshotStore;
            _snapshotter = snapshotter;
            _aggregateRootTypeCodeProvider = aggregateRootTypeCodeProvider;
        }

        public IAggregateRoot Get(Type aggregateRootType, string aggregateRootId)
        {
            if (aggregateRootId == null) throw new ArgumentNullException("aggregateRootId");

            var aggregateRoot = default(IAggregateRoot);

            if (TryGetFromSnapshot(aggregateRootId, aggregateRootType, out aggregateRoot))
            {
                return aggregateRoot;
            }

            var aggregateRootTypeCode = _aggregateRootTypeCodeProvider.GetTypeCode(aggregateRootType);
            var events = _eventStore.QueryAggregateEvents(aggregateRootId, aggregateRootTypeCode, minVersion, maxVersion);
            aggregateRoot = RebuildAggregateRoot(aggregateRootType, events);

            return aggregateRoot;
        }

        #region Helper Methods

        private bool TryGetFromSnapshot(string aggregateRootId, Type aggregateRootType, out IAggregateRoot aggregateRoot)
        {
            aggregateRoot = null;

            var snapshot = _snapshotStore.GetLastestSnapshot(aggregateRootId, aggregateRootType);
            if (snapshot == null) return false;

            var aggregateRootFromSnapshot = _snapshotter.RestoreFromSnapshot(snapshot);
            if (aggregateRootFromSnapshot == null) return false;

            if (aggregateRootFromSnapshot.UniqueId != aggregateRootId)
            {
                throw new Exception(string.Format("Aggregate root restored from snapshot not valid as the aggregate root id not matched. Snapshot aggregate root id:{0}, required aggregate root id:{1}", aggregateRootFromSnapshot.UniqueId, aggregateRootId));
            }

            var aggregateRootTypeCode = _aggregateRootTypeCodeProvider.GetTypeCode(aggregateRootType);
            var eventsAfterSnapshot = _eventStore.QueryAggregateEvents(aggregateRootId, aggregateRootTypeCode, snapshot.Version + 1, int.MaxValue);
            _eventSourcingService.ReplayEvents(aggregateRootFromSnapshot, eventsAfterSnapshot);
            aggregateRoot = aggregateRootFromSnapshot;
            return true;
        }
        private IAggregateRoot RebuildAggregateRoot(Type aggregateRootType, IEnumerable<DomainEventStream> streams)
        {
            var eventStreams = streams.ToList();
            if (streams == null || !eventStreams.Any()) return null;

            var aggregateRoot = _aggregateRootFactory.CreateAggregateRoot(aggregateRootType);
            _eventSourcingService.ReplayEvents(aggregateRoot, eventStreams);

            return aggregateRoot;
        }

        #endregion
    }
}
