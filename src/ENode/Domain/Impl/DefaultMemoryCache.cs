using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ECommon.Logging;
using ECommon.Scheduling;
using ENode.Configurations;
using ENode.Infrastructure;

namespace ENode.Domain.Impl
{
    public class DefaultMemoryCache : IMemoryCache
    {
        private readonly ConcurrentDictionary<string, AggregateCacheInfo> _aggregateRootInfoDict;
        private readonly IAggregateStorage _aggregateStorage;
        private readonly ITypeNameProvider _typeNameProvider;
        private readonly ILogger _logger;
        private readonly IScheduleService _scheduleService;
        private readonly int _timeoutSeconds;
        private readonly string _taskName;

        public DefaultMemoryCache(IScheduleService scheduleService, ITypeNameProvider typeNameProvider, IAggregateStorage aggregateStorage, ILoggerFactory loggerFactory)
        {
            _scheduleService = scheduleService;
            _aggregateRootInfoDict = new ConcurrentDictionary<string, AggregateCacheInfo>();
            _typeNameProvider = typeNameProvider;
            _aggregateStorage = aggregateStorage;
            _logger = loggerFactory.Create(GetType().FullName);
            _timeoutSeconds = ENodeConfiguration.Instance.Setting.AggregateRootMaxInactiveSeconds;
            _taskName = "CleanInactiveAggregates_" + DateTime.Now.Ticks + new Random().Next(10000);
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
        public void Start()
        {
            _scheduleService.StartTask(_taskName, CleanInactiveAggregateRoot, ENodeConfiguration.Instance.Setting.ScanExpiredAggregateIntervalMilliseconds, ENodeConfiguration.Instance.Setting.ScanExpiredAggregateIntervalMilliseconds);
        }
        public void Stop()
        {
            _scheduleService.StopTask(_taskName);
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
                    _logger.DebugFormat("In memory aggregate added, type: {0}, id: {1}, version: {2}", aggregateRoot.GetType().FullName, aggregateRoot.UniqueId, aggregateRoot.Version);
                }
                return new AggregateCacheInfo(aggregateRoot);
            }, (x, existing) =>
            {
                existing.AggregateRoot = aggregateRoot;
                existing.LastUpdateTime = DateTime.Now;
                if (_logger.IsDebugEnabled)
                {
                    _logger.DebugFormat("In memory aggregate updated, type: {0}, id: {1}, version: {2}", aggregateRoot.GetType().FullName, aggregateRoot.UniqueId, aggregateRoot.Version);
                }
                return existing;
            });
        }
        private void CleanInactiveAggregateRoot()
        {
            var inactiveList = new List<KeyValuePair<string, AggregateCacheInfo>>();
            foreach (var pair in _aggregateRootInfoDict)
            {
                if (pair.Value.IsExpired(_timeoutSeconds))
                {
                    inactiveList.Add(pair);
                }
            }
            foreach (var pair in inactiveList)
            {
                AggregateCacheInfo removed;
                if (_aggregateRootInfoDict.TryRemove(pair.Key, out removed))
                {
                    _logger.InfoFormat("Removed inactive aggregate root, id: {0}", pair.Key);
                }
            }
        }
    }
}
