using System.Linq;
using ENode.Eventing;

namespace ENode.Domain.Impl
{
    public class DefaultMemoryCacheRebuilder : IMemoryCacheRebuilder
    {
        #region Private Variables

        private IAggregateRootFactory _aggregateRootFactory;
        private IAggregateRootTypeProvider _aggregateRootTypeProvider;
        private IEventStore _eventStore;
        private IMemoryCache _memoryCache;

        #endregion

        #region Constructors

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

        public void RebuildMemoryCache()
        {
            var groups = _eventStore.QueryAll().GroupBy(x => x.AggregateRootId);
            foreach (var group in groups)
            {
                if (group.Count() == 0) continue;

                var aggregateRootType = _aggregateRootTypeProvider.GetAggregateRootType(group.First().AggregateRootName);
                var aggregateRoot = _aggregateRootFactory.CreateAggregateRoot(aggregateRootType);

                aggregateRoot.ReplayEventStreams(group);

                _memoryCache.Set(aggregateRoot);
            }
        }
    }
}
