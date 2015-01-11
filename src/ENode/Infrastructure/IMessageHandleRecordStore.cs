namespace ENode.Infrastructure
{
    /// <summary>Represents a storage to store the message handle records.
    /// </summary>
    public interface IMessageHandleRecordStore
    {
        /// <summary>Add a message handle record.
        /// </summary>
        /// <param name="record"></param>
        void AddRecord(MessageHandleRecord record);
        /// <summary>Check whether the message handle record exist.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="messageId"></param>
        /// <param name="handlerTypeCode"></param>
        /// <returns></returns>
        bool IsRecordExist(MessageHandleRecordType type, string messageId, int handlerTypeCode);
    }
    public class MessageHandleRecord
    {
        public MessageHandleRecordType Type { get; set; }
        public string MessageId { get; set; }
        public int HandlerTypeCode { get; set; }
        public int MessageTypeCode { get; set; }
        public string AggregateRootId { get; set; }
        public int AggregateRootVersion { get; set; }
    }
    public enum MessageHandleRecordType
    {
        None = 0,
        DomainEvent = 1,
        Event = 2,
        Message = 3,
        PublishableException = 4
    }
}
