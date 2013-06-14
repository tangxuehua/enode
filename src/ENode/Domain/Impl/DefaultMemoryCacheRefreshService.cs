using System;
using ENode.Eventing;

namespace ENode.Domain
{
    public class DefaultMemoryCacheRefreshService : IMemoryCacheRefreshService
    {
        #region Private Variables

        private IRepository _repository;
        private IMemoryCache _memoryCache;
        private IAggregateRootFactory _aggregateRootFactory;
        private IAggregateRootTypeProvider _aggregateRootTypeProvider;

        #endregion

        #region Constructors

        public DefaultMemoryCacheRefreshService(
            IMemoryCache memoryCache,
            IRepository repository,
            IAggregateRootFactory aggregateRootFactory,
            IAggregateRootTypeProvider aggregateRootTypeProvider)
        {
            _memoryCache = memoryCache;
            _repository = repository;
            _aggregateRootFactory = aggregateRootFactory;
            _aggregateRootTypeProvider = aggregateRootTypeProvider;
        }

        #endregion

        public void Refresh(EventStream stream)
        {
            var aggregateRootType = _aggregateRootTypeProvider.GetAggregateRootType(stream.AggregateRootName);

            if (aggregateRootType == null)
            {
                throw new Exception(string.Format("Could not find aggregate root type by aggregate root name {0}", stream.AggregateRootName));
            }

            if (stream.Version == 1)
            {
                var aggregateRoot = _aggregateRootFactory.CreateAggregateRoot(aggregateRootType);
                aggregateRoot.ReplayEvent(stream);
                _memoryCache.Set(aggregateRoot);
            }
            else if (stream.Version > 1)
            {
                var aggregateRoot = _memoryCache.Get(stream.AggregateRootId);

                if (aggregateRoot == null)
                {
                    aggregateRoot = _repository.Get(aggregateRootType, stream.AggregateRootId);
                    _memoryCache.Set(aggregateRoot);
                }
                else if (aggregateRoot.Version + 1 == stream.Version)
                {
                    aggregateRoot.ReplayEvent(stream);
                    _memoryCache.Set(aggregateRoot);
                }
                else if (aggregateRoot.Version + 1 < stream.Version)
                {
                    aggregateRoot = _repository.Get(aggregateRootType, stream.AggregateRootId);
                    _memoryCache.Set(aggregateRoot);
                }
            }
        }
        public void Refresh(Type aggregateRootType, string aggregateRootId)
        {
            var aggregateRoot = _repository.Get(aggregateRootType, aggregateRootId);
            if (aggregateRoot != null)
            {
                _memoryCache.Set(aggregateRoot);
            }
        }
    }
}
