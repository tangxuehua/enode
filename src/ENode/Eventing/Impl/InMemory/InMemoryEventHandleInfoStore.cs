using System.Collections.Concurrent;

namespace ENode.Eventing.Impl.InMemory
{
    public class InMemoryEventHandleInfoStore : IEventHandleInfoStore
    {
        private readonly ConcurrentDictionary<string, int> _versionDict = new ConcurrentDictionary<string, int>();

        public void AddEventHandleInfo(string eventId, int eventHandlerTypeCode, int eventTypeCode, string aggregateRootId, int aggregateRootVersion)
        {
            _versionDict.TryAdd(eventId + eventHandlerTypeCode.ToString(), 0);
        }
        public bool IsEventHandleInfoExist(string eventId, int eventHandlerTypeCode)
        {
            return _versionDict.ContainsKey(eventId + eventHandlerTypeCode.ToString());
        }
    }
}
