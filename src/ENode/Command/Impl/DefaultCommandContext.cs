using System;
using System.Collections.Generic;
using System.Linq;
using ENode.Domain;

namespace ENode.Commanding
{
    public class DefaultCommandContext : ICommandContext, ITrackingContext
    {
        private IList<AggregateRoot> _trackingAggregateRoots;
        private IMemoryCache _memoryCache;
        private IRepository _repository;

        public DefaultCommandContext(IMemoryCache memoryCache, IRepository repository)
        {
            _trackingAggregateRoots = new List<AggregateRoot>();
            _memoryCache = memoryCache;
            _repository = repository;
        }

        public void Add(AggregateRoot aggregateRoot)
        {
            if (aggregateRoot == null)
            {
                throw new ArgumentNullException("aggregateRoot");
            }

            _trackingAggregateRoots.Add(aggregateRoot);
        }
        public T Get<T>(object id) where T : AggregateRoot
        {
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }

            var aggregateRootId = id.ToString();

            var aggregateRoot = _trackingAggregateRoots.SingleOrDefault(x => x.UniqueId == aggregateRootId);
            if (aggregateRoot != null)
            {
                return aggregateRoot as T;
            }

            aggregateRoot = _memoryCache.Get<T>(aggregateRootId);

            if (aggregateRoot == null)
            {
                aggregateRoot = _repository.Get<T>(aggregateRootId);
            }

            if (aggregateRoot == null)
            {
                throw new AggregateRootNotFoundException(aggregateRootId, typeof(T));
            }

            _trackingAggregateRoots.Add(aggregateRoot);

            return aggregateRoot as T;
        }
        public IEnumerable<AggregateRoot> GetTrackedAggregateRoots()
        {
            return _trackingAggregateRoots;
        }
        public void Clear()
        {
            _trackingAggregateRoots.Clear();
        }
    }
}
