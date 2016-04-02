using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ECommon.Logging;
using ENode.Infrastructure;

namespace ENode.Domain.Impl
{
    public class DefaultMemoryCache : IMemoryCache
    {
        private readonly ConcurrentDictionary<string, AggregateCacheInfo> _aggregateRootInfoDict;
        private readonly IAggregateStorage _aggregateStorage;
        private readonly ITypeNameProvider _typeNameProvider;
        private readonly ILogger _logger;

        public DefaultMemoryCache(ITypeNameProvider typeNameProvider, IAggregateStorage aggregateStorage, ILoggerFactory loggerFactory)
        {
            _aggregateRootInfoDict = new ConcurrentDictionary<string, AggregateCacheInfo>();
            _typeNameProvider = typeNameProvider;
            _aggregateStorage = aggregateStorage;
            _logger = loggerFactory.Create(GetType().FullName);
        }

        public IEnumerable<AggregateCacheInfo> GetAll()
        {
            return _aggregateRootInfoDict.Values;
        }
        public IAggregateRoot Get(object aggregateRootId, Type aggregateRootType)
        {
            if (aggregateRootId == null) throw new ArgumentNullException("aggregateRootId");
            AggregateCacheInfo aggregateRootInfo;
            if (_aggregateRootInfoDict.TryGetValue(aggregateRootId.ToString(), out aggregateRootInfo))
            {
                var aggregateRoot = aggregateRootInfo.AggregateRoot;
                if (aggregateRoot.GetType() != aggregateRootType)
                {
                    throw new Exception(string.Format("Incorrect aggregate root type, aggregateRootId:{0}, type:{1}, expecting type:{2}", aggregateRootId, aggregateRoot.GetType(), aggregateRootType));
                }
                if (aggregateRoot.GetChanges().Count() > 0)
                {
                    var lastestAggregateRoot = _aggregateStorage.Get(aggregateRootType, aggregateRootId.ToString());
                    if (lastestAggregateRoot != null)
                    {
                        SetInternal(lastestAggregateRoot);
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
            SetInternal(aggregateRoot);
        }
        public void RefreshAggregateFromEventStore(string aggregateRootTypeName, string aggregateRootId)
        {
            try
            {
                var aggregateRootType = _typeNameProvider.GetType(aggregateRootTypeName);
                if (aggregateRootType == null)
                {
                    _logger.ErrorFormat("Could not find aggregate root type by aggregate root type name [{0}].", aggregateRootTypeName);
                    return;
                }
                var aggregateRoot = _aggregateStorage.Get(aggregateRootType, aggregateRootId);
                if (aggregateRoot != null)
                {
                    SetInternal(aggregateRoot);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Refresh aggregate from event store has unknown exception, aggregateRootTypeName:{0}, aggregateRootId:{1}", aggregateRootTypeName, aggregateRootId), ex);
            }
        }
        public bool Remove(object aggregateRootId)
        {
            if (aggregateRootId == null) throw new ArgumentNullException("aggregateRootId");
            AggregateCacheInfo aggregateRootInfo;
            return _aggregateRootInfoDict.TryRemove(aggregateRootId.ToString(), out aggregateRootInfo);
        }

        private void SetInternal(IAggregateRoot aggregateRoot)
        {
            if (aggregateRoot == null)
            {
                throw new ArgumentNullException("aggregateRoot");
            }
            _aggregateRootInfoDict.AddOrUpdate(aggregateRoot.UniqueId, x =>
            {
                if (_logger.IsDebugEnabled)
                {
                    _logger.DebugFormat("Aggregate memory cache refreshed, type: {0}, id: {1}, version: {2}", aggregateRoot.GetType().FullName, aggregateRoot.UniqueId, aggregateRoot.Version);
                }
                return new AggregateCacheInfo(aggregateRoot);
            }, (x, existing) =>
            {
                existing.AggregateRoot = aggregateRoot;
                existing.LastUpdateTime = DateTime.Now;
                if (_logger.IsDebugEnabled)
                {
                    _logger.DebugFormat("Aggregate memory cache refreshed, type: {0}, id: {1}, version: {2}", aggregateRoot.GetType().FullName, aggregateRoot.UniqueId, aggregateRoot.Version);
                }
                return existing;
            });
        }
    }
}
