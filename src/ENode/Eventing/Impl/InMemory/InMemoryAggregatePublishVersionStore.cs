using System.Collections.Concurrent;

namespace ENode.Eventing.Impl.InMemory
{
    public class InMemoryAggregatePublishVersionStore : IAggregatePublishVersionStore
    {
        private readonly ConcurrentDictionary<string, int> _versionDict = new ConcurrentDictionary<string, int>();

        public void InsertFirstVersion(string eventProcessorName, string aggregateRootId)
        {
            _versionDict.TryAdd(BuildKey(eventProcessorName, aggregateRootId), 1);
        }
        public void UpdateVersion(string eventProcessorName, string aggregateRootId, int version)
        {
            _versionDict[BuildKey(eventProcessorName, aggregateRootId)] = version;
        }
        public int GetVersion(string eventProcessorName, string aggregateRootId)
        {
            int version;
            return _versionDict.TryGetValue(BuildKey(eventProcessorName, aggregateRootId), out version) ? version : 0;
        }

        private string BuildKey(string eventProcessorName, string aggregateRootId)
        {
            return string.Format("{0}-{1}", eventProcessorName, aggregateRootId);
        }
    }
}
