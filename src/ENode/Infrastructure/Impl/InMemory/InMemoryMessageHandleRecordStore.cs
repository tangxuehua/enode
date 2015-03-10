using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace ENode.Infrastructure.Impl.InMemory
{
    public class InMemoryMessageHandleRecordStore : IMessageHandleRecordStore
    {
        private readonly Task<AsyncTaskResult> _successTask = Task.FromResult(AsyncTaskResult.Success);
        private readonly ConcurrentDictionary<string, int> _dict = new ConcurrentDictionary<string, int>();

        Task<AsyncTaskResult> IMessageHandleRecordStore.AddRecordAsync(MessageHandleRecord record)
        {
            _dict.TryAdd(record.MessageId + record.HandlerTypeCode.ToString(), 0);
            return _successTask;
        }
        Task<AsyncTaskResult<bool>> IMessageHandleRecordStore.IsRecordExistAsync(string messageId, int handlerTypeCode)
        {
            return Task.FromResult(new AsyncTaskResult<bool>(AsyncTaskStatus.Success, _dict.ContainsKey(messageId + handlerTypeCode.ToString())));
        }
    }
}
