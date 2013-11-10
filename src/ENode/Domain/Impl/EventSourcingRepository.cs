using System;
using System.Collections.Generic;
using System.Linq;
using ENode.Eventing;
using ENode.Infrastructure;
using ENode.Snapshoting;

namespace ENode.Domain.Impl
{
    /// <summary>An repository implementation with the event sourcing pattern.
    /// </summary>
    public class EventSourcingRepository : IRepository
    {
        private readonly IAggregateRootFactory _aggregateRootFactory;
        private readonly IMemoryCache _memoryCache;
        private readonly IEventSourcingService _eventSourcingService;
        private readonly IEventStore _eventStore;
        private readonly ISnapshotStore _snapshotStore;

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="aggregateRootFactory"></param>
        /// <param name="memoryCache"></param>
        /// <param name="eventSourcingService"></param>
        /// <param name="eventStore"></param>
        /// <param name="snapshotStore"></param>
        public EventSourcingRepository(IAggregateRootFactory aggregateRootFactory, IMemoryCache memoryCache, IEventSourcingService eventSourcingService, IEventStore eventStore, ISnapshotStore snapshotStore)
        {
            _aggregateRootFactory = aggregateRootFactory;
            _memoryCache = memoryCache;
            _eventSourcingService = eventSourcingService;
            _eventStore = eventStore;
            _snapshotStore = snapshotStore;
        }

        /// <summary>Get an aggregate from memory cache, if not exist, get it from event store.
        /// </summary>
        /// <param name="id"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>(object id) where T : class, IAggregateRoot
        {
            return Get(typeof(T), id) as T;
        }
        /// <summary>Get an aggregate from memory cache, if not exist, get it from event store.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public IAggregateRoot Get(Type type, object id)
        {
            if (id == null) throw new ArgumentNullException("id");
            return _memoryCache.Get(id, type) ?? GetFromStorage(type, id.ToString());
        }

        #region Helper Methods

        /// <summary>Get aggregate root from event store.
        /// </summary>
        private IAggregateRoot GetFromStorage(Type aggregateRootType, object aggregateRootId)
        {
            IAggregateRoot aggregateRoot;
            const long minStreamVersion = 1;
            const long maxStreamVersion = long.MaxValue;

            if (TryGetFromSnapshot(aggregateRootId, aggregateRootType, out aggregateRoot))
            {
                return aggregateRoot;
            }

            var streams = _eventStore.Query(aggregateRootId, aggregateRootType, minStreamVersion, maxStreamVersion);
            aggregateRoot = BuildAggregateRoot(aggregateRootType, streams);

            return aggregateRoot;
        }
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
            _eventSourcingService.ReplayEventStream(aggregateRootFromSnapshot, eventsAfterSnapshot);
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
            _eventSourcingService.ReplayEventStream(aggregateRoot, eventStreams);

            return aggregateRoot;
        }

        #endregion
    }
}
