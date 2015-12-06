using System.Collections.Concurrent;
using System.Threading.Tasks;
using ECommon.IO;

namespace ENode.Infrastructure.Impl.InMemory
{
    public class InMemoryMessageHandleRecordStore : IMessageHandleRecordStore
    {
        private readonly Task<AsyncTaskResult> _successTask = Task.FromResult(AsyncTaskResult.Success);
        private readonly ConcurrentDictionary<string, int> _dict = new ConcurrentDictionary<string, int>();

        Task<AsyncTaskResult> IMessageHandleRecordStore.AddRecordAsync(MessageHandleRecord record)
        {
            _dict.TryAdd(record.MessageId + record.HandlerTypeName, 0);
            return _successTask;
        }
        public Task<AsyncTaskResult> AddRecordAsync(TwoMessageHandleRecord record)
        {
            _dict.TryAdd(record.MessageId1 + record.MessageId2 + record.HandlerTypeName, 0);
            return _successTask;
        }
        public Task<AsyncTaskResult> AddRecordAsync(ThreeMessageHandleRecord record)
        {
            _dict.TryAdd(record.MessageId1 + record.MessageId2 + record.MessageId3 + record.HandlerTypeName, 0);
            return _successTask;
        }
        Task<AsyncTaskResult<bool>> IMessageHandleRecordStore.IsRecordExistAsync(string messageId, string handlerTypeName, string aggregateRootTypeName)
        {
            return Task.FromResult(new AsyncTaskResult<bool>(AsyncTaskStatus.Success, _dict.ContainsKey(messageId + handlerTypeName)));
        }
        public Task<AsyncTaskResult<bool>> IsRecordExistAsync(string messageId1, string messageId2, string handlerTypeName, string aggregateRootTypeName)
        {
            return Task.FromResult(new AsyncTaskResult<bool>(AsyncTaskStatus.Success, _dict.ContainsKey(messageId1 + messageId2 + handlerTypeName)));
        }
        public Task<AsyncTaskResult<bool>> IsRecordExistAsync(string messageId1, string messageId2, string messageId3, string handlerTypeName, string aggregateRootTypeName)
        {
            return Task.FromResult(new AsyncTaskResult<bool>(AsyncTaskStatus.Success, _dict.ContainsKey(messageId1 + messageId2 + messageId3 + handlerTypeName)));
        }
    }
}
