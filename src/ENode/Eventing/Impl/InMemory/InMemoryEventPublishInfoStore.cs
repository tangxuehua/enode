using System.Collections.Concurrent;

namespace ENode.Eventing.Impl.InMemory
{
    /// <summary>Represents a storage to store the event publish information of aggregate.
    /// </summary>
    public class InMemoryEventPublishInfoStore : IEventPublishInfoStore
    {
        private ConcurrentDictionary<string, long> _versionDict = new ConcurrentDictionary<string, long>();

        public void InsertFirstPublishedVersion(string aggregateRootId)
        {
            _versionDict.TryAdd(aggregateRootId, 1);
        }

        public void UpdatePublishedVersion(string aggregateRootId, long version)
        {
            _versionDict[aggregateRootId] = version;
        }

        public long GetEventPublishedVersion(string aggregateRootId)
        {
            return _versionDict[aggregateRootId];
        }
    }
}
