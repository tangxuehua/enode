using System.Linq;
using ENode.Eventing;

namespace ENode.Domain.Impl
{
    /// <summary>
    /// 
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

        /// <summary>
        /// 
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

        /// <summary>
        /// 
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
