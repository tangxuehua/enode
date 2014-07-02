using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ENode.Eventing.Impl.InMemory
{
    /// <summary>Local in-memory implementation of IEventHandleInfoCache using ConcurrentDictionary.
    /// </summary>
    public class InMemoryEventHandleInfoCache : IEventHandleInfoCache
    {
        private readonly ConcurrentDictionary<string, IList<int>> _handleInfoDict = new ConcurrentDictionary<string, IList<int>>();

        /// <summary>Insert an event handle info to the cache.
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="eventHandlerTypeCode"></param>
        /// <param name="eventTypeCode"></param>
        /// <param name="aggregateRootId"></param>
        /// <param name="aggregateRootVersion"></param>
        public void AddEventHandleInfo(string eventId, int eventHandlerTypeCode, int eventTypeCode, string aggregateRootId, int aggregateRootVersion)
        {
            IList<int> handlerTypeCodeList;
            if (!_handleInfoDict.TryGetValue(eventId, out handlerTypeCodeList))
            {
                handlerTypeCodeList = new List<int>();
            }
            handlerTypeCodeList.Add(eventHandlerTypeCode);
        }
        /// <summary>Check whether the given event was handled by the given event handler.
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="eventHandlerTypeCode"></param>
        /// <returns></returns>
        public bool IsEventHandleInfoExist(string eventId, int eventHandlerTypeCode)
        {
            IList<int> handlerTypeCodeList;
            return _handleInfoDict.TryGetValue(eventId, out handlerTypeCodeList) && handlerTypeCodeList.Contains(eventHandlerTypeCode);
        }
        /// <summary>Remove all the event handle information from the cache by the given eventId.
        /// </summary>
        /// <param name="eventId"></param>
        public void RemoveEventHandleInfo(string eventId)
        {
            IList<int> handlerTypeCodeList;
            _handleInfoDict.TryRemove(eventId, out handlerTypeCodeList);
        }
    }
}
