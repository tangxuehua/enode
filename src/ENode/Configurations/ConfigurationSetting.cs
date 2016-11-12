namespace ENode.Configurations
{
    public class ConfigurationSetting
    {
        /// <summary>处理领域事件的处理器的名字；默认为DefaultEventProcessor
        /// </summary>
        public string DomainEventStreamMessageHandlerName { get; set; }
        /// <summary>默认的数据库配置信息
        /// </summary>
        public DefaultDBConfigurationSetting DefaultDBConfigurationSetting { get; set; }
        /// <summary>当使用默认的从内存清理聚合根的服务时，该属性用于配置扫描过期的聚合根的时间间隔，默认为5秒；
        /// </summary>
        public int ScanExpiredAggregateIntervalMilliseconds { get; set; }
        /// <summary>当使用默认的MemoryCache时，该属性用于配置聚合根的最长允许的不活跃时间，超过这个时间就认为是过期，就可以从内存清除了；然后下次如果再需要用的时候再重新加载进来；默认为3天；
        /// </summary>
        public int AggregateRootMaxInactiveSeconds { get; set; }
        /// <summary>CommandMailBox中的命令处理时一次最多处理多少个命令，默认为1000个
        /// </summary>
        public int CommandMailBoxPersistenceMaxBatchSize { get; set; }
        /// <summary>EventMailBox中的事件持久化时一次最多持久化多少个事件，默认为1000个
        /// </summary>
        public int EventMailBoxPersistenceMaxBatchSize { get; set; }

        public ConfigurationSetting() : this(null) { }
        public ConfigurationSetting(string connectionString = null)
        {
            DomainEventStreamMessageHandlerName = "DefaultEventProcessor";
            DefaultDBConfigurationSetting = new DefaultDBConfigurationSetting(connectionString);
            ScanExpiredAggregateIntervalMilliseconds = 5000;
            AggregateRootMaxInactiveSeconds = 3600 * 24 * 3;
            CommandMailBoxPersistenceMaxBatchSize = 1000;
            EventMailBoxPersistenceMaxBatchSize = 1000;
        }
    }
    public class DefaultDBConfigurationSetting
    {
        /// <summary>数据库连接字符串
        /// </summary>
        public string ConnectionString { get; set; }
        /// <summary>命令表的默认名称；默认为：Command
        /// </summary>
        public string CommandTableName { get; set; }
        /// <summary>事件表的默认名称；默认为：EventStream
        /// </summary>
        public string EventTableName { get; set; }
        /// <summary>事件表的默认个数，用于支持最简易的单库分表；默认为：1，即不分表
        /// </summary>
        public int EventTableCount { get; set; }
        /// <summary>事件表批量持久化单批最大事件数；默认为：1000
        /// </summary>
        public int EventTableBulkCopyBatchSize { get; set; }
        /// <summary>事件表批量持久化单批超时时间；单位为秒，默认为：60s
        /// </summary>
        public int EventTableBulkCopyTimeout { get; set; }
        /// <summary>聚合根已发布事件表的默认名称；默认为：PublishedVersion
        /// </summary>
        public string PublishedVersionTableName { get; set; }
        /// <summary>LockKey表的默认名称；默认为：LockKey
        /// </summary>
        public string LockKeyTableName { get; set; }
        /// <summary>Command表的CommandId的唯一索引的默认名称；默认为：IX_Command_CommandId
        /// </summary>
        public string CommandTableCommandIdUniqueIndexName { get; set; }
        /// <summary>事件表的聚合根版本唯一索引的默认名称；默认为：IX_EventStream_AggId_Version
        /// </summary>
        public string EventTableVersionUniqueIndexName { get; set; }
        /// <summary>事件表的聚合根已处理命令唯一索引的默认名称；默认为：IX_EventStream_AggId_CommandId
        /// </summary>
        public string EventTableCommandIdUniqueIndexName { get; set; }
        /// <summary>聚合根已发布事件表的聚合根已发布版本唯一索引的默认名称；默认为：IX_PublishedVersion_AggId_Version
        /// </summary>
        public string PublishedVersionUniqueIndexName { get; set; }
        /// <summary>LockKey表的默认主键的名称；默认为：PK_LockKey
        /// </summary>
        public string LockKeyPrimaryKeyName { get; set; }

        public DefaultDBConfigurationSetting(string connectionString = null)
        {
            ConnectionString = connectionString;
            CommandTableName = "Command";
            EventTableName = "EventStream";
            EventTableCount = 1;
            EventTableBulkCopyBatchSize = 1000;
            EventTableBulkCopyTimeout = 60;
            PublishedVersionTableName = "PublishedVersion";
            LockKeyTableName = "LockKey";
            CommandTableCommandIdUniqueIndexName = "IX_Command_CommandId";
            EventTableVersionUniqueIndexName = "IX_EventStream_AggId_Version";
            EventTableCommandIdUniqueIndexName = "IX_EventStream_AggId_CommandId";
            PublishedVersionUniqueIndexName = "IX_PublishedVersion_AggId_Version";
            LockKeyPrimaryKeyName = "PK_LockKey";
        }
    }
}
