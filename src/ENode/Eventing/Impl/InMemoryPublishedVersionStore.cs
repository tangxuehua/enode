using System.Collections.Concurrent;
using System.Threading.Tasks;
using ECommon.IO;

namespace ENode.Eventing.Impl
{
    public class InMemoryPublishedVersionStore : IPublishedVersionStore
    {
        private readonly Task<AsyncTaskResult> _successTask = Task.FromResult(AsyncTaskResult.Success);
        private readonly ConcurrentDictionary<string, int> _versionDict = new ConcurrentDictionary<string, int>();

        public Task<AsyncTaskResult> UpdatePublishedVersionAsync(string processorName, string aggregateRootTypeName, string aggregateRootId, int publishedVersion)
        {
            _versionDict[BuildKey(processorName, aggregateRootId)] = publishedVersion;
            return _successTask;
        }
        public Task<AsyncTaskResult<int>> GetPublishedVersionAsync(string processorName, string aggregateRootTypeName, string aggregateRootId)
        {
            int version;
            var publishedVersion = _versionDict.TryGetValue(BuildKey(processorName, aggregateRootId), out version) ? version : 0;
            return Task.FromResult(new AsyncTaskResult<int>(AsyncTaskStatus.Success, publishedVersion));
        }

        private string BuildKey(string eventProcessorName, string aggregateRootId)
        {
            return string.Format("{0}-{1}", eventProcessorName, aggregateRootId);
        }
    }
}
