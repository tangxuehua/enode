using System.Linq;
using ENode.Eventing;

namespace ENode.Domain.Impl
{
    /// <summary>Default implementation of IMemoryCacheRebuilder.
    /// </summary>
    public class DefaultMemoryCacheRebuilder : IMemoryCacheRebuilder
    {
        #region Private Variables

        private readonly IAggregateRootFactory _aggregateRootFactory;
        private readonly IAggregateRootTypeProvider _aggregateRootTypeProvider;
        private readonly IEventStore _eventStore;
        private readonly IMemoryCache _memoryCache;

        #endregion

        #region Constructors

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="aggregateRootFactory"></param>
        /// <param name="aggregateRootTypeProvider"></param>
        /// <param name="eventStore"></param>
        /// <param name="memoryCache"></param>
        public DefaultMemoryCacheRebuilder(
            IAggregateRootFactory aggregateRootFactory,
            IAggregateRootTypeProvider aggregateRootTypeProvider,
            IEventStore eventStore,
            IMemoryCache memoryCache)
        {
            _aggregateRootFactory = aggregateRootFactory;
            _aggregateRootTypeProvider = aggregateRootTypeProvider;
            _eventStore = eventStore;
            _memoryCache = memoryCache;
        }

        #endregion

        /// <summary>Using event sourcing pattern to rebuild the whole domain by replaying all the domain events from the eventstore.
        /// </summary>
        public void RebuildMemoryCache()
        {
            var groups = _eventStore.QueryAll().GroupBy(x => x.AggregateRootId);
            foreach (var group in groups)
            {
                if (!group.Any()) continue;

                var aggregateRootType = _aggregateRootTypeProvider.GetAggregateRootType(group.First().AggregateRootName);
                var aggregateRoot = _aggregateRootFactory.CreateAggregateRoot(aggregateRootType);

                aggregateRoot.ReplayEventStreams(group);

                _memoryCache.Set(aggregateRoot);
            }
        }
    }
}
