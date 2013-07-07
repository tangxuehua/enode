using System;

namespace ENode.Messaging.Storage.MongoDB
{
    public class DefaultQueueCollectionNameProvider : IQueueCollectionNameProvider
    {
        private string _queueNameFormat;

        public DefaultQueueCollectionNameProvider(string queueNameFormat)
        {
            _queueNameFormat = queueNameFormat;
        }
        public string GetCollectionName(string queueName)
        {
            if (!string.IsNullOrEmpty(_queueNameFormat))
            {
                return string.Format(_queueNameFormat, queueName);
            }
            return queueName;
        }
    }
}
