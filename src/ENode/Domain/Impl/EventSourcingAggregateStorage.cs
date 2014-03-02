using System;
using System.Collections.Generic;
using System.Linq;
using ECommon.IoC;
using ENode.Eventing;
using ENode.Infrastructure;
using ENode.Snapshoting;

namespace ENode.Domain.Impl
{
    /// <summary>An aggregate storage implementation with the event sourcing pattern.
    /// </summary>
    public class EventSourcingAggregateStorage : IAggregateStorage
    {
        private const long minStreamVersion = 1;
        private const long maxStreamVersion = long.MaxValue;
        private readonly IAggregateRootFactory _aggregateRootFactory;
        private readonly IEventStreamConvertService _eventStreamConvertService;
        private readonly IEventSourcingService _eventSourcingService;
        private readonly IEventStore _eventStore;
        private readonly ISnapshotStore _snapshotStore;
        private readonly IAggregateRootTypeProvider _aggregateRootTypeProvider;

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="aggregateRootFactory"></param>
        /// <param name="eventStreamConvertService"></param>
        /// <param name="eventSourcingService"></param>
        /// <param name="eventStore"></param>
        /// <param name="snapshotStore"></param>
        /// <param name="aggregateRootTypeProvider"></param>
        public EventSourcingAggregateStorage(
            IAggregateRootFactory aggregateRootFactory,
            IEventStreamConvertService eventStreamConvertService,
            IEventSourcingService eventSourcingService,
            IEventStore eventStore,
            ISnapshotStore snapshotStore,
            IAggregateRootTypeProvider aggregateRootTypeProvider)
        {
            _aggregateRootFactory = aggregateRootFactory;
            _eventStreamConvertService = eventStreamConvertService;
            _eventSourcingService = eventSourcingService;
            _eventStore = eventStore;
            _snapshotStore = snapshotStore;
            _aggregateRootTypeProvider = aggregateRootTypeProvider;
        }

        /// <summary>Get an aggregate from aggregate storage.
        /// </summary>
        /// <param name="aggregateRootType"></param>
        /// <param name="aggregateRootId"></param>
        /// <returns></returns>
        public IAggregateRoot Get(Type aggregateRootType, string aggregateRootId)
        {
            if (aggregateRootId == null) throw new ArgumentNullException("aggregateRootId");

            var aggregateRoot = default(IAggregateRoot);

            if (TryGetFromSnapshot(aggregateRootId, aggregateRootType, out aggregateRoot))
            {
                return aggregateRoot;
            }

            var aggregateRootName = _aggregateRootTypeProvider.GetAggregateRootTypeName(aggregateRootType);
            var streams = _eventStore.Query(aggregateRootId, aggregateRootName, minStreamVersion, maxStreamVersion);
            aggregateRoot = BuildAggregateRoot(aggregateRootType, streams.Select(x => _eventStreamConvertService.ConvertFrom(x)));

            return aggregateRoot;
        }

        #region Helper Methods

        /// <summary>Try to get an aggregate root from snapshot store.
        /// </summary>
        private bool TryGetFromSnapshot(string aggregateRootId, Type aggregateRootType, out IAggregateRoot aggregateRoot)
        {
            aggregateRoot = null;

            var snapshot = _snapshotStore.GetLastestSnapshot(aggregateRootId, aggregateRootType);
            if (snapshot == null) return false;

            var aggregateRootFromSnapshot = ObjectContainer.Resolve<ISnapshotter>().RestoreFromSnapshot(snapshot);
            if (aggregateRootFromSnapshot == null) return false;

            if (aggregateRootFromSnapshot.UniqueId != aggregateRootId)
            {
                throw new ENodeException("Aggregate root restored from snapshot not valid as the aggregate root id not matched. Snapshot aggregate root id:{0}, required aggregate root id:{1}", aggregateRootFromSnapshot.UniqueId, aggregateRootId);
            }

            var aggregateRootName = _aggregateRootTypeProvider.GetAggregateRootTypeName(aggregateRootType);
            var eventsAfterSnapshot = _eventStore.Query(aggregateRootId, aggregateRootName, snapshot.Version + 1, long.MaxValue);
            _eventSourcingService.ReplayEvents(aggregateRootFromSnapshot, eventsAfterSnapshot.Select(x => _eventStreamConvertService.ConvertFrom(x)));
            aggregateRoot = aggregateRootFromSnapshot;
            return true;
        }
        /// <summary>Rebuild the aggregate root using the event sourcing pattern.
        /// </summary>
        private IAggregateRoot BuildAggregateRoot(Type aggregateRootType, IEnumerable<EventStream> streams)
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
