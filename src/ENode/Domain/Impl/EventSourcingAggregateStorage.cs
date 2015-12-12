using System;
using System.Collections.Generic;
using System.Linq;
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

        public IAggregateRoot Get(Type aggregateRootType, string aggregateRootId)
        {
            if (aggregateRootType == null) throw new ArgumentNullException("aggregateRootType");
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
            aggregateRoot = _aggregateSnapshotter.RestoreFromSnapshot(aggregateRootType, aggregateRootId);

            if (aggregateRoot == null) return false;

            if (aggregateRoot.GetType() != aggregateRootType || aggregateRoot.UniqueId != aggregateRootId)
            {
                throw new Exception(string.Format("AggregateRoot recovery from snapshot is invalid as the aggregateRootType or aggregateRootId is not matched. Snapshot: [aggregateRootType:{0},aggregateRootId:{1}], expected: [aggregateRootType:{2},aggregateRootId:{3}]",
                    aggregateRoot.GetType(),
                    aggregateRoot.UniqueId,
                    aggregateRootType,
                    aggregateRootId));
            }

            var aggregateRootTypeName = _typeNameProvider.GetTypeName(aggregateRootType);
            var eventStreamsAfterSnapshot = _eventStore.QueryAggregateEvents(aggregateRootId, aggregateRootTypeName, aggregateRoot.Version + 1, int.MaxValue);
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
