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
        public string SqlDefaultConnectionString { get; set; }
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

            DomainEventStreamMessageHandlerName = "DefaultEventHandler";

            SqlServerLockServiceSetting = new DbTableSetting(this);
            SqlServerCommandStoreSetting = new DbTableSetting(this);
            SqlServerEventStoreSetting = new DbTableSetting(this);
            SqlServerSequenceMessagePublishedVersionStoreSetting = new DbTableSetting(this);
            SqlServerMessageHandleRecordStoreSetting = new DbTableSetting(this);

            SqlServerLockServiceSetting.SetOptionValue("TableName", "LockKey");

            SqlServerCommandStoreSetting.SetOptionValue("TableName", "Command");
            SqlServerCommandStoreSetting.SetOptionValue("PrimaryKeyName", "PK_Command");

            SqlServerEventStoreSetting.SetOptionValue("TableName", "EventStream");
            SqlServerEventStoreSetting.SetOptionValue("PrimaryKeyName", "PK_EventStream");
            SqlServerEventStoreSetting.SetOptionValue("CommandIndexName", "IX_EventStream_AggId_CommandId");
            SqlServerEventStoreSetting.SetOptionValue("BulkCopyBatchSize", 1000);
            SqlServerEventStoreSetting.SetOptionValue("BulkCopyTimeout", 60);

            SqlServerSequenceMessagePublishedVersionStoreSetting.SetOptionValue("TableName", "SequenceMessagePublishedVersion");
            SqlServerSequenceMessagePublishedVersionStoreSetting.SetOptionValue("PrimaryKeyName", "PK_SequenceMessagePublishedVersion");

            SqlServerMessageHandleRecordStoreSetting.SetOptionValue("OneMessageTableName", "MessageHandleRecord");
            SqlServerMessageHandleRecordStoreSetting.SetOptionValue("TwoMessageTableName", "TwoMessageHandleRecord");
            SqlServerMessageHandleRecordStoreSetting.SetOptionValue("ThreeMessageTableName", "ThreeMessageHandleRecord");
            SqlServerMessageHandleRecordStoreSetting.SetOptionValue("OneMessageTablePrimaryKeyName", "PK_MessageHandleRecord");
            SqlServerMessageHandleRecordStoreSetting.SetOptionValue("TwoMessageTablePrimaryKeyName", "PK_TwoMessageHandleRecord");
            SqlServerMessageHandleRecordStoreSetting.SetOptionValue("ThreeMessageTablePrimaryKeyName", "PK_ThreeMessageHandleRecord");
        }
    }
}
