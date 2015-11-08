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
        public bool UseCodeAttribute { get; set; }

        public ConfigurationSetting()
        {
            EnableGroupCommitEvent = false;
            GroupCommitEventInterval = 100;
            GroupCommitMaxSize = 1000;
            ImmediatelyRetryTimes = 3;
            RetryIntervalForIOException = 1000;
            DomainEventStreamMessageHandlerName = "DefaultEventHandler";
            UseCodeAttribute = true;
        }
    }
}
