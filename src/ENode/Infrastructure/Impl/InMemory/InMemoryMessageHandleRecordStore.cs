using System.Collections.Concurrent;

namespace ENode.Infrastructure.Impl.InMemory
{
    public class InMemoryMessageHandleRecordStore : IMessageHandleRecordStore
    {
        private readonly ConcurrentDictionary<string, int> _dict = new ConcurrentDictionary<string, int>();

        public void AddRecord(MessageHandleRecord record)
        {
            _dict.TryAdd(record.MessageId + record.HandlerTypeCode.ToString(), 0);
        }
        public bool IsRecordExist(MessageHandleRecordType type, string messageId, int handlerTypeCode)
        {
            return _dict.ContainsKey(messageId + handlerTypeCode.ToString());
        }
    }
}
