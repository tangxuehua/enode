using System;

namespace ENode.Messaging.Storage.MongoDB
{
    public class DefaultQueueCollectionNameProvider : IQueueCollectionNameProvider
    {
        private string _collectionNameFormat;

        public DefaultQueueCollectionNameProvider(string collectionNameFormat)
        {
            _collectionNameFormat = collectionNameFormat;
        }
        public string GetCollectionName(string queueName)
        {
            if (!string.IsNullOrEmpty(_collectionNameFormat))
            {
                return string.Format(_collectionNameFormat, queueName);
            }
            return queueName;
        }
    }
}
