namespace ENode.Configurations
{
    public class ConfigurationSetting
    {
        /// <summary>处理领域事件的处理器的名字；DefaultEventProcessor
        /// </summary>
        public string DomainEventProcessorName { get; set; }
        /// <summary>当使用默认的从内存清理聚合根的服务时，该属性用于配置扫描过期的聚合根的时间间隔，默认为5秒；
        /// </summary>
        public int ScanExpiredAggregateIntervalMilliseconds { get; set; }
        /// <summary>定时获取CQRS的Q端的聚合根的EventMailBox的最新已处理的事件的版本号的定时任务的时间间隔，默认为5秒；
        /// </summary>
        public int ProcessTryToRefreshAggregateIntervalMilliseconds { get; set; }
        /// <summary>当使用默认的MemoryCache时，该属性用于配置聚合根的最长允许的不活跃时间，超过这个时间就认为是过期，就可以从内存清除了；然后下次如果再需要用的时候再重新加载进来；默认为3天；
        /// </summary>
        public int AggregateRootMaxInactiveSeconds { get; set; }
        /// <summary>CommandMailBox中的命令处理时一次最多处理多少个，默认为1000个
        /// </summary>
        public int CommandMailBoxProcessBatchSize { get; set; }
        /// <summary>EventMailBox的个数，默认为4个
        /// </summary>
        public int EventMailBoxCount { get; set; }
        /// <summary>EventMailBox中的事件持久化时一次最多持久化多少个事件，默认为1000个
        /// </summary>
        public int EventMailBoxPersistenceMaxBatchSize { get; set; }

        public ConfigurationSetting()
        {
            DomainEventProcessorName = "DefaultEventProcessor";
            ScanExpiredAggregateIntervalMilliseconds = 5000;
            ProcessTryToRefreshAggregateIntervalMilliseconds = 5000;
            AggregateRootMaxInactiveSeconds = 3600 * 24 * 3;
            CommandMailBoxProcessBatchSize = 1000;
            EventMailBoxCount = 4;
            EventMailBoxPersistenceMaxBatchSize = 1000;
        }
    }
}
