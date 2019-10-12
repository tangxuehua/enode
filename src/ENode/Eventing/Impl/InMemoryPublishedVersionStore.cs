using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace ENode.Eventing.Impl
{
    public class InMemoryPublishedVersionStore : IPublishedVersionStore
    {
        private readonly ConcurrentDictionary<string, int> _versionDict = new ConcurrentDictionary<string, int>();

        public Task UpdatePublishedVersionAsync(string processorName, string aggregateRootTypeName, string aggregateRootId, int publishedVersion)
        {
            _versionDict[BuildKey(processorName, aggregateRootId)] = publishedVersion;
            return Task.CompletedTask;
        }
        public Task<int> GetPublishedVersionAsync(string processorName, string aggregateRootTypeName, string aggregateRootId)
        {
            var publishedVersion = _versionDict.TryGetValue(BuildKey(processorName, aggregateRootId), out int version) ? version : 0;
            return Task.FromResult(publishedVersion);
        }

        private string BuildKey(string eventProcessorName, string aggregateRootId)
        {
            return string.Format("{0}-{1}", eventProcessorName, aggregateRootId);
        }
    }
}
