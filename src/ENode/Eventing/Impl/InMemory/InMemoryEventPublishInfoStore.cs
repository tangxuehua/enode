using System.Collections.Concurrent;

namespace ENode.Eventing.Impl.InMemory
{
    /// <summary>Represents a storage to store the event publish information of aggregate.
    /// </summary>
    public class InMemoryEventPublishInfoStore : IEventPublishInfoStore
    {
        private readonly ConcurrentDictionary<string, int> _versionDict = new ConcurrentDictionary<string, int>();

        /// <summary>Insert the first published event version of aggregate.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        public void InsertPublishedVersion(string aggregateRootId)
        {
            _versionDict.TryAdd(aggregateRootId, 1);
        }

        /// <summary>Update the published event version of aggregate.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        /// <param name="version"></param>
        public void UpdatePublishedVersion(string aggregateRootId, int version)
        {
            _versionDict[aggregateRootId] = version;
        }

        /// <summary>Get the current event published version for the specified aggregate.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        /// <returns></returns>
        public int GetEventPublishedVersion(string aggregateRootId)
        {
            int version;
            return _versionDict.TryGetValue(aggregateRootId, out version) ? version : 0;
        }
    }
}
