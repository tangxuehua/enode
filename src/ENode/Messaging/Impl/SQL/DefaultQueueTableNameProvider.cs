namespace ENode.Messaging
{
    public class DefaultQueueTableNameProvider : IQueueTableNameProvider
    {
        private string _tableNameFormat;

        public DefaultQueueTableNameProvider(string tableNameFormat)
        {
            _tableNameFormat = tableNameFormat;
        }
        public string GetTable(string queueName)
        {
            if (!string.IsNullOrEmpty(_tableNameFormat))
            {
                return string.Format(_tableNameFormat, queueName);
            }
            return queueName;
        }
    }
}
