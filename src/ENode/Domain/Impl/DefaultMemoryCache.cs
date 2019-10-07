using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ECommon.Logging;
using ECommon.Scheduling;
using ENode.Configurations;
using ENode.Infrastructure;

namespace ENode.Domain.Impl
{
    public class DefaultMemoryCache : IMemoryCache
    {
        private object _lockObj = new object();
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

        public async Task<IAggregateRoot> GetAsync(object aggregateRootId, Type aggregateRootType)
        {
            if (aggregateRootId == null) throw new ArgumentNullException("aggregateRootId");
            if (aggregateRootType == null) throw new ArgumentNullException("aggregateRootType");

            if (_aggregateRootInfoDict.TryGetValue(aggregateRootId.ToString(), out AggregateCacheInfo aggregateRootInfo))
            {
                var aggregateRoot = aggregateRootInfo.AggregateRoot;
                if (aggregateRoot.GetType() != aggregateRootType)
                {
                    throw new Exception(string.Format("Incorrect aggregate root type, aggregateRootId:{0}, type:{1}, expecting type:{2}", aggregateRootId, aggregateRoot.GetType(), aggregateRootType));
                }
                if (aggregateRoot.GetChanges().Count() > 0)
                {
                    var lastestAggregateRoot = await _aggregateStorage.GetAsync(aggregateRootType, aggregateRootId.ToString()).ConfigureAwait(false);
                    if (lastestAggregateRoot != null)
                    {
                        ResetAggregateRootCache(lastestAggregateRoot);
                    }
                    return lastestAggregateRoot;
                }
                return aggregateRoot;
            }
            return null;
        }
        public async Task<T> GetAsync<T>(object aggregateRootId) where T : class, IAggregateRoot
        {
            return await GetAsync(aggregateRootId, typeof(T)).ConfigureAwait(false) as T;
        }
        public Task UpdateAggregateRootCache(IAggregateRoot aggregateRoot)
        {
            ResetAggregateRootCache(aggregateRoot);
            return Task.CompletedTask;
        }
        public Task<IAggregateRoot> RefreshAggregateFromEventStoreAsync(string aggregateRootTypeName, object aggregateRootId)
        {
            if (aggregateRootTypeName == null) throw new ArgumentNullException("aggregateRootTypeName");

            var aggregateRootType = _typeNameProvider.GetType(aggregateRootTypeName);
            if (aggregateRootType == null)
            {
                _logger.ErrorFormat("Could not find aggregate root type by aggregate root type name [{0}].", aggregateRootTypeName);
                return null;
            }
            return RefreshAggregateFromEventStoreAsync(aggregateRootType, aggregateRootId);
        }
        public async Task<IAggregateRoot> RefreshAggregateFromEventStoreAsync(Type aggregateRootType, object aggregateRootId)
        {
            if (aggregateRootId == null) throw new ArgumentNullException("aggregateRootId");
            if (aggregateRootType == null) throw new ArgumentNullException("aggregateRootType");

            try
            {
                var aggregateRoot = await _aggregateStorage.GetAsync(aggregateRootType, aggregateRootId.ToString()).ConfigureAwait(false);
                if (aggregateRoot != null)
                {
                    ResetAggregateRootCache(aggregateRoot);
                }
                return aggregateRoot;
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Refresh aggregate from event store has unknown exception, aggregateRootTypeName:{0}, aggregateRootId:{1}", _typeNameProvider.GetTypeName(aggregateRootType), aggregateRootId), ex);
                return null;
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

        private void ResetAggregateRootCache(IAggregateRoot aggregateRoot)
        {
            lock (_lockObj)
            {
                if (aggregateRoot == null)
                {
                    throw new ArgumentNullException("aggregateRoot");
                }
                _aggregateRootInfoDict.AddOrUpdate(aggregateRoot.UniqueId, x =>
                {
                    _logger.DebugFormat("Aggregate root in-memory cache init, aggregateRootType: {0}, aggregateRootId: {1}, aggregateRootVersion: {2}", aggregateRoot.GetType().FullName, aggregateRoot.UniqueId, aggregateRoot.Version);
                    return new AggregateCacheInfo(aggregateRoot);
                }, (x, existing) =>
                {
                    var aggregateRootOldVersion = existing.AggregateRoot.Version;
                    existing.AggregateRoot = aggregateRoot;
                    existing.LastUpdateTime = DateTime.Now;
                    _logger.DebugFormat("Aggregate root in-memory cache reset, aggregateRootType: {0}, aggregateRootId: {1}, aggregateRootNewVersion: {2}, aggregateRootOldVersion: {3}", aggregateRoot.GetType().FullName, aggregateRoot.UniqueId, aggregateRoot.Version, aggregateRootOldVersion);
                    return existing;
                });
            }
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
