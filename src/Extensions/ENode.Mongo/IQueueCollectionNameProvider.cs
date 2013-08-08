namespace ENode.Mongo
{
    /// <summary>Represents a provider to provide the queue collection name.
    /// </summary>
    public interface IQueueCollectionNameProvider
    {
        /// <summary>Get mongo collection name for the given queue.
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns></returns>
        string GetCollectionName(string queueName);
    }
}
