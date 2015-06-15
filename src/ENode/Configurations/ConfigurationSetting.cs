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
        public int SqlServerBulkCopyBatchSize { get; set; }
        public int SqlServerBulkCopyTimeout { get; set; }
        public string SqlServerDefaultConnectionString { get; set; }
        public string DomainEventStreamMessageHandlerName { get; set; }

        public DbTableSetting SqlServerLockServiceSetting { get; set; }
        public DbTableSetting SqlServerCommandStoreSetting { get; set; }
        public DbTableSetting SqlServerEventStoreSetting { get; set; }
        public DbTableSetting SqlServerSequenceMessagePublishedVersionStoreSetting { get; set; }
        public DbTableSetting SqlServerMessageHandleRecordStoreSetting { get; set; }

        public ConfigurationSetting()
        {
            EnableGroupCommitEvent = false;
            GroupCommitEventInterval = 100;
            GroupCommitMaxSize = 1000;
            ImmediatelyRetryTimes = 3;
            RetryIntervalForIOException = 1000;

            DomainEventStreamMessageHandlerName = "DefaultDomainEventStreamMessageHandler";

            SqlServerBulkCopyBatchSize = 1000;
            SqlServerBulkCopyTimeout = 60;
            SqlServerLockServiceSetting = new DbTableSetting(this) { TableName = "Lock" };
            SqlServerCommandStoreSetting = new DbTableSetting(this) { TableName = "Command", PrimaryKeyName = "PK_Command" };
            SqlServerEventStoreSetting = new DbTableSetting(this) { TableName = "EventStream", PrimaryKeyName = "PK_EventStream", CommandIndexName = "IX_EventStream_AggId_CommandId" };
            SqlServerSequenceMessagePublishedVersionStoreSetting = new DbTableSetting(this) { TableName = "SequenceMessagePublishedVersion", PrimaryKeyName = "PK_SequenceMessagePublishedVersion" };
            SqlServerMessageHandleRecordStoreSetting = new DbTableSetting(this) { TableName = "MessageHandleRecord", PrimaryKeyName = "PK_MessageHandleRecord" };
        }
    }
}
