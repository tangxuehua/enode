using System;
using System.Collections.Concurrent;
using System.Linq;
using ECommon.Logging;
using ENode.Infrastructure;

namespace ENode.Domain.Impl
{
    public class DefaultMemoryCache : IMemoryCache
    {
        private readonly ConcurrentDictionary<string, IAggregateRoot> _aggregateRootDict = new ConcurrentDictionary<string, IAggregateRoot>();
        private readonly IAggregateStorage _aggregateStorage;
        private readonly ITypeCodeProvider _aggregateRootTypeCodeProvider;
        private readonly ILogger _logger;

        public DefaultMemoryCache(ITypeCodeProvider aggregateRootTypeCodeProvider, IAggregateStorage aggregateStorage, ILoggerFactory loggerFactory)
        {
            _aggregateRootTypeCodeProvider = aggregateRootTypeCodeProvider;
            _aggregateStorage = aggregateStorage;
            _logger = loggerFactory.Create(GetType().FullName);
        }

        public IAggregateRoot Get(object aggregateRootId, Type aggregateRootType)
        {
            if (aggregateRootId == null) throw new ArgumentNullException("aggregateRootId");
            IAggregateRoot aggregateRoot;
            if (_aggregateRootDict.TryGetValue(aggregateRootId.ToString(), out aggregateRoot))
            {
                if (aggregateRoot.GetType() != aggregateRootType)
                {
                    throw new Exception(string.Format("Incorrect aggregate root type, current aggregateRootId:{0}, type:{1}, expecting type:{2}", aggregateRootId, aggregateRoot.GetType(), aggregateRootType));
                }
                if (aggregateRoot.GetChanges().Count() > 0)
                {
                    var lastestAggregateRoot = _aggregateStorage.Get(aggregateRootType, aggregateRootId.ToString());
                    if (lastestAggregateRoot != null)
                    {
                        _aggregateRootDict[aggregateRoot.UniqueId] = lastestAggregateRoot;
                    }
                    return lastestAggregateRoot;
                }
                return aggregateRoot;
            }
            return null;
        }
        public T Get<T>(object aggregateRootId) where T : class, IAggregateRoot
        {
            return Get(aggregateRootId, typeof(T)) as T;
        }
        public void Set(IAggregateRoot aggregateRoot)
        {
            if (aggregateRoot == null)
            {
                throw new ArgumentNullException("aggregateRoot");
            }
            _aggregateRootDict[aggregateRoot.UniqueId] = aggregateRoot;
        }
        public void RefreshAggregateFromEventStore(int aggregateRootTypeCode, string aggregateRootId)
        {
            try
            {
                var aggregateRootType = _aggregateRootTypeCodeProvider.GetType(aggregateRootTypeCode);
                if (aggregateRootType == null)
                {
                    _logger.ErrorFormat("Could not find aggregate root type by aggregate root type code [{0}].", aggregateRootTypeCode);
                    return;
                }
                var aggregateRoot = _aggregateStorage.Get(aggregateRootType, aggregateRootId);
                if (aggregateRoot != null)
                {
                    _aggregateRootDict[aggregateRoot.UniqueId] = aggregateRoot;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Exception raised when refreshing aggregate from event store, aggregateRootTypeCode:{0}, aggregateRootId:{1}", aggregateRootTypeCode, aggregateRootId), ex);
            }
        }
    }
}
