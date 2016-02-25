using System;
using System.Collections.Concurrent;
using System.Linq;
using ECommon.Logging;
using ECommon.Scheduling;
using ENode.Configurations;
using ENode.Infrastructure;

namespace ENode.Domain.Impl
{
    //TODO，将Clean过期聚合根的事情交给一个独立的CleanExpiredAggregateService去做；
    //该服务中还需要将Aggregate对应的EventMailBox从内存删除；
    public class DefaultMemoryCache : IMemoryCache
    {
        class AggregateRootInfo
        {
            public IAggregateRoot AggregateRoot;
            public DateTime LastUpdateTime;

            public bool IsExpired(int timeoutSeconds)
            {
                return (DateTime.Now - LastUpdateTime).TotalSeconds >= timeoutSeconds;
            }
        }
        private readonly ConcurrentDictionary<string, AggregateRootInfo> _aggregateRootInfoDict = new ConcurrentDictionary<string, AggregateRootInfo>();
        private readonly IAggregateStorage _aggregateStorage;
        private readonly ITypeNameProvider _typeNameProvider;
        private readonly IScheduleService _scheduleService;
        private readonly ILogger _logger;
        private readonly int TimeoutSeconds = 1800;

        public DefaultMemoryCache(ITypeNameProvider typeNameProvider, IAggregateStorage aggregateStorage, IScheduleService scheduleService, ILoggerFactory loggerFactory)
        {
            _typeNameProvider = typeNameProvider;
            _aggregateStorage = aggregateStorage;
            _scheduleService = scheduleService;
            _logger = loggerFactory.Create(GetType().FullName);
            TimeoutSeconds = ENodeConfiguration.Instance.Setting.AggregateRootMaxInactiveSeconds;
            _scheduleService.StartTask("RemoveExpiredAggregates", RemoveExpiredAggregates, 1000, ENodeConfiguration.Instance.Setting.ScanExpiredAggregateIntervalMilliseconds);
        }

        public IAggregateRoot Get(object aggregateRootId, Type aggregateRootType)
        {
            if (aggregateRootId == null) throw new ArgumentNullException("aggregateRootId");
            AggregateRootInfo aggregateRootInfo;
            if (_aggregateRootInfoDict.TryGetValue(aggregateRootId.ToString(), out aggregateRootInfo))
            {
                var aggregateRoot = aggregateRootInfo.AggregateRoot;
                if (aggregateRoot.GetType() != aggregateRootType)
                {
                    throw new Exception(string.Format("Incorrect aggregate root type, current aggregateRootId:{0}, type:{1}, expecting type:{2}", aggregateRootId, aggregateRoot.GetType(), aggregateRootType));
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
                _logger.Error(string.Format("Exception raised when refreshing aggregate from event store, aggregateRootTypeName:{0}, aggregateRootId:{1}", aggregateRootTypeName, aggregateRootId), ex);
            }
        }
        public bool Remove(object aggregateRootId)
        {
            if (aggregateRootId == null) throw new ArgumentNullException("aggregateRootId");
            AggregateRootInfo aggregateRootInfo;
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
                    _logger.DebugFormat("Added aggregate root to memory cache, type: {0}, id: {1}, version: {2}", aggregateRoot.GetType().FullName, aggregateRoot.UniqueId, aggregateRoot.Version);
                }
                return new AggregateRootInfo { AggregateRoot = aggregateRoot, LastUpdateTime = DateTime.Now };
            }, (x, existing) =>
            {
                existing.AggregateRoot = aggregateRoot;
                existing.LastUpdateTime = DateTime.Now;
                if (_logger.IsDebugEnabled)
                {
                    _logger.DebugFormat("Updated aggregate root to memory cache, type: {0}, id: {1}, version: {2}", aggregateRoot.GetType().FullName, aggregateRoot.UniqueId, aggregateRoot.Version);
                }
                return existing;
            });
        }
        private void RemoveExpiredAggregates()
        {
            var expiredAggregateRootInfos = _aggregateRootInfoDict.Values.Where(x => x.IsExpired(TimeoutSeconds));
            foreach (var aggregateRootInfo in expiredAggregateRootInfos)
            {
                AggregateRootInfo removed;
                if (_aggregateRootInfoDict.TryRemove(aggregateRootInfo.AggregateRoot.UniqueId, out removed))
                {
                    if (_logger.IsDebugEnabled)
                    {
                        _logger.DebugFormat("Removed expired aggregate root from memory cache, type: {0}, id: {1}, version: {2}, lastActiveTime: {3}", removed.AggregateRoot.GetType().FullName, removed.AggregateRoot.UniqueId, removed.AggregateRoot.Version, removed.LastUpdateTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                    }
                }
            }
        }
    }
}
