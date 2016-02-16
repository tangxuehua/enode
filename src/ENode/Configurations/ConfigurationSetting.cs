using System;

namespace ENode.Configurations
{
    public class ConfigurationSetting
    {
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
        /// <summary>事件持久化队列个数，默认为CPU核数乘以2，每个队列会有一个固定的线程去持久化该队列中的事件。
        /// </summary>
        public int EventPersistQueueCount { get; set; }

        public ConfigurationSetting()
        {
            DomainEventStreamMessageHandlerName = "DefaultEventHandler";
            ScanExpiredAggregateIntervalMilliseconds = 5000;
            AggregateRootMaxInactiveSeconds = 3600 * 24 * 3;
            EventPersistQueueCount = Environment.ProcessorCount * 2;
        }
    }
}
