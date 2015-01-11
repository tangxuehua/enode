using System.Collections.Concurrent;
using System.Collections.Generic;
using ECommon.Extensions;

namespace ENode.Infrastructure.Impl.InMemory
{
    public class InMemoryMessageHandleRecordCache : IMessageHandleRecordCache
    {
        private readonly ConcurrentDictionary<string, ISet<int>> _dict = new ConcurrentDictionary<string, ISet<int>>();

        public void AddRecord(MessageHandleRecord record)
        {
            _dict.GetOrAdd(record.MessageId, new HashSet<int>()).Add(record.HandlerTypeCode);
        }
        public bool IsRecordExist(MessageHandleRecordType type, string messageId, int handlerTypeCode)
        {
            ISet<int> handlerTypeCodeList;
            return _dict.TryGetValue(messageId, out handlerTypeCodeList) && handlerTypeCodeList.Contains(handlerTypeCode);
        }
        public void RemoveRecordFromCache(MessageHandleRecordType type, string messageId)
        {
            _dict.Remove(messageId);
        }
    }
}
