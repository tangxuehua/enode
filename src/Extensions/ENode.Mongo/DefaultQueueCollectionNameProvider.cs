namespace ENode.Mongo
{
    /// <summary>The default implementation of IQueueCollectionNameProvider.
    /// </summary>
    public class DefaultQueueCollectionNameProvider : IQueueCollectionNameProvider
    {
        private readonly string _queueNameFormat;

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="queueNameFormat"></param>
        public DefaultQueueCollectionNameProvider(string queueNameFormat)
        {
            _queueNameFormat = queueNameFormat;
        }
        /// <summary>Get mongo collection name for the given queue.
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns></returns>
        public string GetCollectionName(string queueName)
        {
            return !string.IsNullOrEmpty(_queueNameFormat) ? string.Format(_queueNameFormat, queueName) : queueName;
        }
    }
}
