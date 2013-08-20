using System.Collections.Concurrent;

namespace ENode.Eventing.Impl.InMemory
{
    /// <summary>Represents a storage to store the event publish information of aggregate.
    /// </summary>
    public class InMemoryEventPublishInfoStore : IEventPublishInfoStore
    {
        private readonly ConcurrentDictionary<string, long> _versionDict = new ConcurrentDictionary<string, long>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aggregateRootId"></param>
        public void InsertFirstPublishedVersion(string aggregateRootId)
        {
            _versionDict.TryAdd(aggregateRootId, 1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aggregateRootId"></param>
        /// <param name="version"></param>
        public void UpdatePublishedVersion(string aggregateRootId, long version)
        {
            _versionDict[aggregateRootId] = version;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aggregateRootId"></param>
        /// <returns></returns>
        public long GetEventPublishedVersion(string aggregateRootId)
        {
            return _versionDict[aggregateRootId];
        }
    }
}
