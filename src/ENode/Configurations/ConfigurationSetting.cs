namespace ENode.Configurations
{
    public class ConfigurationSetting
    {
        public bool EnableGroupCommitEvent { get; set; }
        public int GroupCommitEventInterval { get; set; }
        public int ImmediatelyRetryTimes { get; set; }
        public int RetryIntervalForIOException { get; set; }
        public int CommandProcessorParallelThreadCount { get; set; }
        public int EventProcessorParallelThreadCount { get; set; }
        public int ExceptionProcessorParallelThreadCount { get; set; }
        public int MessageProcessorParallelThreadCount { get; set; }
        public string SqlServerDefaultConnectionString { get; set; }
        public DbTableSetting SqlServerLockServiceSetting { get; set; }
        public DbTableSetting SqlServerCommandStoreSetting { get; set; }
        public DbTableSetting SqlServerEventStoreSetting { get; set; }
        public DbTableSetting SqlServerEventPublishInfoStoreSetting { get; set; }
        public DbTableSetting SqlServerEventHandleInfoStoreSetting { get; set; }

        public ConfigurationSetting()
        {
            EnableGroupCommitEvent = false;
            GroupCommitEventInterval = 10;
            ImmediatelyRetryTimes = 3;
            RetryIntervalForIOException = 1000;
            CommandProcessorParallelThreadCount = 100;
            EventProcessorParallelThreadCount = 4;
            ExceptionProcessorParallelThreadCount = 1;
            MessageProcessorParallelThreadCount = 1;
            SqlServerLockServiceSetting = new DbTableSetting(this) { TableName = "Lock" };
            SqlServerCommandStoreSetting = new DbTableSetting(this) { TableName = "Command", PrimaryKeyName = "PK_Command" };
            SqlServerEventStoreSetting = new DbTableSetting(this) { TableName = "EventStream", PrimaryKeyName = "PK_EventStream" };
            SqlServerEventPublishInfoStoreSetting = new DbTableSetting(this) { TableName = "EventPublishInfo", PrimaryKeyName = "PK_EventPublishInfo" };
            SqlServerEventHandleInfoStoreSetting = new DbTableSetting(this) { TableName = "EventHandleInfo" };
        }
    }
}
