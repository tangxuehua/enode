namespace ENode.Messaging.Impl.SQL
{
    /// <summary>The default implementation of IQueueTableNameProvider.
    /// </summary>
    public class DefaultQueueTableNameProvider : IQueueTableNameProvider
    {
        private readonly string _tableNameFormat;

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="tableNameFormat"></param>
        public DefaultQueueTableNameProvider(string tableNameFormat)
        {
            _tableNameFormat = tableNameFormat;
        }
        /// <summary>Get the formatted table name by the given queue name and the current table name format.
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns></returns>
        public string GetTable(string queueName)
        {
            return !string.IsNullOrEmpty(_tableNameFormat) ? string.Format(_tableNameFormat, queueName) : queueName;
        }
    }
}
