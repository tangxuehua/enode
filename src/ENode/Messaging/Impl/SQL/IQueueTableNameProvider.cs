namespace ENode.Messaging
{
    /// <summary>Represents a provider to provide the queue table name.
    /// </summary>
    public interface IQueueTableNameProvider
    {
        /// <summary>Get table for the given queue.
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns></returns>
        string GetTable(string queueName);
    }
}
