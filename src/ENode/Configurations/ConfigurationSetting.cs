using System;

namespace ENode.Configurations
{
    public class ConfigurationSetting
    {
        /// <summary>是否支持批量持久化事件，默认为False；
        /// </summary>
        public bool EnableGroupCommitEvent { get; set; }
        /// <summary>批量持久化事件的时间间隔，毫秒为单位，默认为100毫秒；
        /// </summary>
        public int GroupCommitEventIntervalMilliseconds { get; set; }
        /// <summary>批量持久化一次最大持久化的事件数，默认为1000个；
        /// </summary>
        public int GroupCommitMaxSize { get; set; }
        /// <summary>处理领域事件的处理器的名字，默认为DefaultEventHandler
        /// </summary>
        public string DomainEventStreamMessageHandlerName { get; set; }
        /// <summary>ENode数据库的默认数据库连接字符串；
        /// </summary>
        public string SqlDefaultConnectionString { get; set; }
        /// <summary>当使用默认的MemoryCache时，该属性用于配置扫描过期的聚合根的时间间隔，默认为5秒；
        /// </summary>
        public int ScanExpiredAggregateIntervalMilliseconds { get; set; }
        /// <summary>当使用默认的MemoryCache时，该属性用于配置聚合根的最长允许的不活跃时间，超过这个时间就认为是过期，就可以从内存清除了；然后下次如果再需要用的时候再重新加载进来；默认为3天；
        /// </summary>
        public int AggregateRootMaxInactiveSeconds { get; set; }

        public ConfigurationSetting()
        {
            EnableGroupCommitEvent = false;
            GroupCommitEventIntervalMilliseconds = 100;
            GroupCommitMaxSize = 1000;
            DomainEventStreamMessageHandlerName = "DefaultEventHandler";
            ScanExpiredAggregateIntervalMilliseconds = 5000;
            AggregateRootMaxInactiveSeconds = 3600 * 24 * 3;
        }
    }
}
