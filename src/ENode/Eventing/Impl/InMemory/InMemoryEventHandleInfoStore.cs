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
        /// <param name="eventHandlerTypeCode"></param>
        /// <param name="eventTypeCode"></param>
        /// <param name="aggregateRootId"></param>
        /// <param name="aggregateRootVersion"></param>
        public void AddEventHandleInfo(string eventId, int eventHandlerTypeCode, int eventTypeCode, string aggregateRootId, int aggregateRootVersion)
        {
            _versionDict.TryAdd(eventId + eventHandlerTypeCode.ToString(), 0);
        }
        /// <summary>Check whether the given event was handled by the given event handler.
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="eventHandlerTypeCode"></param>
        /// <returns></returns>
        public bool IsEventHandleInfoExist(string eventId, int eventHandlerTypeCode)
        {
            return _versionDict.ContainsKey(eventId + eventHandlerTypeCode.ToString());
        }
    }
}
