using System.Linq;
using ECommon.Scheduling;
using ENode.Configurations;

namespace ENode.Domain.Impl
{
    public class DefaultCleanAggregateService : ICleanAggregateService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IScheduleService _scheduleService;
        private readonly int TimeoutSeconds = 1800;

        public DefaultCleanAggregateService(IMemoryCache memoryCache, IScheduleService scheduleService)
        {
            TimeoutSeconds = ENodeConfiguration.Instance.Setting.AggregateRootMaxInactiveSeconds;
            _memoryCache = memoryCache;
            _scheduleService = scheduleService;
            _scheduleService.StartTask("CleanAggregates", Clean, 1000, ENodeConfiguration.Instance.Setting.ScanExpiredAggregateIntervalMilliseconds);
        }

        public void Clean()
        {
            var expiredAggregateRootInfos = _memoryCache.GetAll().Where(x => x.IsExpired(TimeoutSeconds));
            foreach (var aggregateRootInfo in expiredAggregateRootInfos)
            {
                _memoryCache.Remove(aggregateRootInfo.AggregateRoot.UniqueId);
            }
        }
    }
}
