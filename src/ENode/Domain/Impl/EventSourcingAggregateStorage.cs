using System;
using System.Collections.Generic;
using System.Linq;
using ECommon.IoC;
using ENode.Eventing;
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
        private readonly IEventSourcingService _eventSourcingService;
        private readonly IEventStore _eventStore;
        private readonly ISnapshotStore _snapshotStore;

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="aggregateRootFactory"></param>
        /// <param name="eventSourcingService"></param>
        /// <param name="eventStore"></param>
        /// <param name="snapshotStore"></param>
        public EventSourcingAggregateStorage(IAggregateRootFactory aggregateRootFactory, IEventSourcingService eventSourcingService, IEventStore eventStore, ISnapshotStore snapshotStore)
        {
            _aggregateRootFactory = aggregateRootFactory;
            _eventSourcingService = eventSourcingService;
            _eventStore = eventStore;
            _snapshotStore = snapshotStore;
        }

        /// <summary>Get an aggregate from memory cache, if not exist, get it from event store.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public IAggregateRoot Get(Type type, object id)
        {
            if (id == null) throw new ArgumentNullException("id");

            var aggregateRootId = id.ToString();
            var aggregateRoot = default(IAggregateRoot);

            if (TryGetFromSnapshot(aggregateRootId, type, out aggregateRoot))
            {
                return aggregateRoot;
            }

            var streams = _eventStore.Query(aggregateRootId, type, minStreamVersion, maxStreamVersion);
            aggregateRoot = BuildAggregateRoot(type, streams);

            return aggregateRoot;
        }

        #region Helper Methods

        /// <summary>Try to get an aggregate root from snapshot store.
        /// </summary>
        private bool TryGetFromSnapshot(object aggregateRootId, Type aggregateRootType, out IAggregateRoot aggregateRoot)
        {
            aggregateRoot = null;

            var snapshot = _snapshotStore.GetLastestSnapshot(aggregateRootId, aggregateRootType);
            if (snapshot == null) return false;

            var aggregateRootFromSnapshot = ObjectContainer.Resolve<ISnapshotter>().RestoreFromSnapshot(snapshot);
            if (aggregateRootFromSnapshot == null) return false;

            if (aggregateRootFromSnapshot.UniqueId != aggregateRootId)
            {
                var message = string.Format("Aggregate root restored from snapshot not valid as the aggregate root id not matched. Snapshot aggregate root id:{0}, required aggregate root id:{1}", aggregateRootFromSnapshot.UniqueId, aggregateRootId);
                throw new Exception(message);
            }

            var eventsAfterSnapshot = _eventStore.Query(aggregateRootId, aggregateRootType, snapshot.Version + 1, long.MaxValue);
            _eventSourcingService.ReplayEvents(aggregateRootFromSnapshot, eventsAfterSnapshot);
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
