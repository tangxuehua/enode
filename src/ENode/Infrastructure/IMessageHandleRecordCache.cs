namespace ENode.Infrastructure
{
    /// <summary>Represents a in-memory cache to store the message handle records.
    /// </summary>
    public interface IMessageHandleRecordCache : IMessageHandleRecordStore
    {
        /// <summary>Remove all the message handle records from memory cache by messageId.
        /// </summary>
        /// <param name="messageId"></param>
        void RemoveRecordFromCache(MessageHandleRecordType type, string messageId);
    }
}
