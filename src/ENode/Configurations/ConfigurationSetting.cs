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
        public string DomainEventStreamMessageHandlerName { get; set; }

        public string SqlDefaultConnectionString { get; set; }
        public DbTableSetting SqlLockServiceSetting { get; set; }
        public DbTableSetting SqlCommandStoreSetting { get; set; }
        public DbTableSetting SqlEventStoreSetting { get; set; }
        public DbTableSetting SqlSequenceMessagePublishedVersionStoreSetting { get; set; }
        public DbTableSetting SqlMessageHandleRecordStoreSetting { get; set; }

        public ConfigurationSetting()
        {
            EnableGroupCommitEvent = false;
            GroupCommitEventInterval = 100;
            GroupCommitMaxSize = 1000;
            ImmediatelyRetryTimes = 3;
            RetryIntervalForIOException = 1000;

            DomainEventStreamMessageHandlerName = "DefaultEventHandler";

            SqlLockServiceSetting = new DbTableSetting(this);
            SqlCommandStoreSetting = new DbTableSetting(this);
            SqlEventStoreSetting = new DbTableSetting(this);
            SqlSequenceMessagePublishedVersionStoreSetting = new DbTableSetting(this);
            SqlMessageHandleRecordStoreSetting = new DbTableSetting(this);

            SqlLockServiceSetting.SetOptionValue("TableName", "LockKey");

            SqlCommandStoreSetting.SetOptionValue("TableName", "Command");
            SqlCommandStoreSetting.SetOptionValue("PrimaryKeyName", "PK_Command");

            SqlEventStoreSetting.SetOptionValue("TableName", "EventStream");
            SqlEventStoreSetting.SetOptionValue("PrimaryKeyName", "PK_EventStream");
            SqlEventStoreSetting.SetOptionValue("CommandIndexName", "IX_EventStream_AggId_CommandId");
            SqlEventStoreSetting.SetOptionValue("BulkCopyBatchSize", 1000);
            SqlEventStoreSetting.SetOptionValue("BulkCopyTimeout", 60);

            SqlSequenceMessagePublishedVersionStoreSetting.SetOptionValue("TableName", "SequenceMessagePublishedVersion");
            SqlSequenceMessagePublishedVersionStoreSetting.SetOptionValue("PrimaryKeyName", "PK_SequenceMessagePublishedVersion");

            SqlMessageHandleRecordStoreSetting.SetOptionValue("OneMessageTableName", "MessageHandleRecord");
            SqlMessageHandleRecordStoreSetting.SetOptionValue("TwoMessageTableName", "TwoMessageHandleRecord");
            SqlMessageHandleRecordStoreSetting.SetOptionValue("ThreeMessageTableName", "ThreeMessageHandleRecord");
            SqlMessageHandleRecordStoreSetting.SetOptionValue("OneMessageTablePrimaryKeyName", "PK_MessageHandleRecord");
            SqlMessageHandleRecordStoreSetting.SetOptionValue("TwoMessageTablePrimaryKeyName", "PK_TwoMessageHandleRecord");
            SqlMessageHandleRecordStoreSetting.SetOptionValue("ThreeMessageTablePrimaryKeyName", "PK_ThreeMessageHandleRecord");
        }
    }
}
