using System;
using System.Collections.Concurrent;

namespace ENode.Eventing.Impl.InMemory
{
    /// <summary>Local in-memory implementation of IEventHandleInfoStore using ConcurrentDictionary.
    /// </summary>
    public class InMemoryEventHandleInfoStore : IEventHandleInfoStore
    {
        private readonly ConcurrentDictionary<string, int> _versionDict = new ConcurrentDictionary<string, int>();

        /// <summary>Insert an event handle info.
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="eventHandlerTypeName"></param>
        public void AddEventHandleInfo(string eventId, string eventHandlerTypeName)
        {
            _versionDict.TryAdd(eventId + eventHandlerTypeName, 0);
        }
        /// <summary>Check whether the given event was handled by the given event handler.
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="eventHandlerTypeName"></param>
        /// <returns></returns>
        public bool IsEventHandleInfoExist(string eventId, string eventHandlerTypeName)
        {
            return _versionDict.ContainsKey(eventId + eventHandlerTypeName);
        }
    }
}
