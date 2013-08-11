using System;
using System.Collections.Generic;
using System.Linq;
using ENode.Eventing;
using ENode.Infrastructure;
using ENode.Snapshoting;

namespace ENode.Domain {
    public class EventSourcingRepository : IRepository {
        private IAggregateRootTypeProvider _aggregateRootTypeProvider;
        private IAggregateRootFactory _aggregateRootFactory;
        private IMemoryCache _memoryCache;
        private IEventStore _eventStore;
        private ISnapshotStore _snapshotStore;

        public EventSourcingRepository(IAggregateRootTypeProvider aggregateRootTypeProvider, IAggregateRootFactory aggregateRootFactory, IMemoryCache memoryCache, IEventStore eventStore, ISnapshotStore snapshotStore) {
            _aggregateRootTypeProvider = aggregateRootTypeProvider;
            _aggregateRootFactory = aggregateRootFactory;
            _memoryCache = memoryCache;
            _eventStore = eventStore;
            _snapshotStore = snapshotStore;
        }

        public T Get<T>(string id) where T : AggregateRoot {
            return Get(typeof(T), id) as T;
        }
        public AggregateRoot Get(Type type, string id) {
            var aggregateRoot = _memoryCache.Get(id);
            if (aggregateRoot == null) {
                aggregateRoot = GetFromStorage(type, id);
            }
            return aggregateRoot;
        }

        #region Helper Methods

        /// <summary>Get aggregate root from data storage.
        /// </summary>
        private AggregateRoot GetFromStorage(Type aggregateRootType, string aggregateRootId) {
            AggregateRoot aggregateRoot = null;
            long minStreamVersion = 1;
            long maxStreamVersion = long.MaxValue;

            if (TryGetFromSnapshot(aggregateRootId, aggregateRootType, out aggregateRoot)) {
                return aggregateRoot;
            }
            else {
                var streams = _eventStore.Query(aggregateRootId, aggregateRootType, minStreamVersion, maxStreamVersion);
                aggregateRoot = BuildAggregateRoot(aggregateRootId, aggregateRootType, streams);
            }

            return aggregateRoot;
        }
        /// <summary>Try to get an aggregate root from snapshot store.
        /// </summary>
        private bool TryGetFromSnapshot(string aggregateRootId, Type aggregateRootType, out AggregateRoot aggregateRoot) {
            aggregateRoot = null;

            var snapshot = _snapshotStore.GetLastestSnapshot(aggregateRootId, aggregateRootType);
            if (snapshot != null) {
                AggregateRoot aggregateRootFromSnapshot = ObjectContainer.Resolve<ISnapshotter>().RestoreFromSnapshot(snapshot);
                if (aggregateRootFromSnapshot != null) {
                    if (aggregateRootFromSnapshot.UniqueId != aggregateRootId) {
                        string message = string.Format("从快照还原出来的聚合根的Id({0})与所要求的Id({1})不符", aggregateRootFromSnapshot.UniqueId, aggregateRootId);
                        throw new Exception(message);
                    }
                    var commitsAfterSnapshot = _eventStore.Query(aggregateRootId, aggregateRootType, snapshot.StreamVersion + 1, long.MaxValue);
                    aggregateRootFromSnapshot.ReplayEventStreams(commitsAfterSnapshot);
                    aggregateRoot = aggregateRootFromSnapshot;
                    return true;
                }
            }

            return false;
        }
        /// <summary>Rebuild the aggregate root using the event sourcing pattern.
        /// </summary>
        private AggregateRoot BuildAggregateRoot(string aggregateRootId, Type aggregateRootType, IEnumerable<EventStream> streams) {
            AggregateRoot aggregateRoot = null;

            if (streams != null && streams.Count() > 0) {
                aggregateRoot = _aggregateRootFactory.CreateAggregateRoot(aggregateRootType);
                aggregateRoot.ReplayEventStreams(streams);
            }

            return aggregateRoot;
        }

        #endregion
    }
}
