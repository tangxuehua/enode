using System;

namespace ENode.Mongo
{
    /// <summary>
    /// 
    /// </summary>
    public class DefaultQueueCollectionNameProvider : IQueueCollectionNameProvider
    {
        private readonly string _queueNameFormat;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queueNameFormat"></param>
        public DefaultQueueCollectionNameProvider(string queueNameFormat)
        {
            _queueNameFormat = queueNameFormat;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns></returns>
        public string GetCollectionName(string queueName)
        {
            return !string.IsNullOrEmpty(_queueNameFormat) ? string.Format(_queueNameFormat, queueName) : queueName;
        }
    }
}
