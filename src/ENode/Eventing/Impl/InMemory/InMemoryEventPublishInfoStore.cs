using System.Collections.Concurrent;

namespace ENode.Eventing.Impl.InMemory
{
    /// <summary>Represents a storage to store the event publish information of aggregate.
    /// </summary>
    public class InMemoryEventPublishInfoStore : IEventPublishInfoStore
    {
        private readonly ConcurrentDictionary<string, long> _versionDict = new ConcurrentDictionary<string, long>();

        /// <summary>Insert the first published event version of aggregate.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        public void InsertFirstPublishedVersion(object aggregateRootId)
        {
            _versionDict.TryAdd(aggregateRootId.ToString(), 1);
        }

        /// <summary>Update the published event version of aggregate.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        /// <param name="version"></param>
        public void UpdatePublishedVersion(object aggregateRootId, long version)
        {
            _versionDict[aggregateRootId.ToString()] = version;
        }

        /// <summary>Get the current event published version for the specified aggregate.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        /// <returns></returns>
        public long GetEventPublishedVersion(object aggregateRootId)
        {
            long version;
            return _versionDict.TryGetValue(aggregateRootId.ToString(), out version) ? version : 0L;
        }
    }
}
