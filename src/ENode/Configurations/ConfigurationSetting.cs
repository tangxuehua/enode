using System;

namespace ENode.Configurations
{
    public class ConfigurationSetting
    {
        public bool EnableGroupCommitEvent { get; set; }
        public int GroupCommitEventInterval { get; set; }
        public int GroupCommitMaxSize { get; set; }
        public int ImmediatelyRetryTimes { get; set; }
        public int RetryIntervalForIOException { get; set; }
        public int AggregateCommandProcessorParallelThreadCount { get; set; }
        public int ApplicationCommandProcessorParallelThreadCount { get; set; }
        public int DomainEventStreamProcessorParallelThreadCount { get; set; }
        public int EventStreamProcessorParallelThreadCount { get; set; }
        public int EventProcessorParallelThreadCount { get; set; }
        public int ExceptionProcessorParallelThreadCount { get; set; }
        public int MessageProcessorParallelThreadCount { get; set; }
        public int SqlServerBulkCopyBatchSize { get; set; }
        public int SqlServerBulkCopyTimeout { get; set; }
        public string SqlServerDefaultConnectionString { get; set; }

        public string DomainEventStreamMessageHandlerName { get; set; }

        public DbTableSetting SqlServerLockServiceSetting { get; set; }
        public DbTableSetting SqlServerCommandStoreSetting { get; set; }
        public DbTableSetting SqlServerEventStoreSetting { get; set; }
        public DbTableSetting SqlServerAggregatePublishVersionStoreSetting { get; set; }
        public DbTableSetting SqlServerMessageHandleRecordStoreSetting { get; set; }

        public ConfigurationSetting()
        {
            EnableGroupCommitEvent = false;
            GroupCommitEventInterval = 100;
            GroupCommitMaxSize = 1000;
            ImmediatelyRetryTimes = 3;
            RetryIntervalForIOException = 1000;
            AggregateCommandProcessorParallelThreadCount = Environment.ProcessorCount;
            ApplicationCommandProcessorParallelThreadCount = Environment.ProcessorCount;
            DomainEventStreamProcessorParallelThreadCount = Environment.ProcessorCount;
            EventStreamProcessorParallelThreadCount = 1;
            EventProcessorParallelThreadCount = 1;
            ExceptionProcessorParallelThreadCount = 1;
            MessageProcessorParallelThreadCount = 1;

            DomainEventStreamMessageHandlerName = "DefaultDomainEventStreamMessageHandler";

            SqlServerBulkCopyBatchSize = 1000;
            SqlServerBulkCopyTimeout = 60;
            SqlServerLockServiceSetting = new DbTableSetting(this) { TableName = "Lock" };
            SqlServerCommandStoreSetting = new DbTableSetting(this) { TableName = "Command", PrimaryKeyName = "PK_Command" };
            SqlServerEventStoreSetting = new DbTableSetting(this) { TableName = "EventStream", PrimaryKeyName = "PK_EventStream" };
            SqlServerAggregatePublishVersionStoreSetting = new DbTableSetting(this) { TableName = "SequenceMessagePublishedVersion", PrimaryKeyName = "PK_SequenceMessagePublishedVersion" };
            SqlServerMessageHandleRecordStoreSetting = new DbTableSetting(this) { TableName = "MessageHandleRecord", PrimaryKeyName = "PK_MessageHandleRecord" };
        }
    }
}
