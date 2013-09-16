using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ENode.Eventing.Impl.InMemory
{
    /// <summary>Local in-memory implementation of IEventHandleInfoCache using ConcurrentDictionary.
    /// </summary>
    public class InMemoryEventHandleInfoCache : IEventHandleInfoCache
    {
        private readonly ConcurrentDictionary<Guid, IList<string>> _handleInfoDict = new ConcurrentDictionary<Guid, IList<string>>();

        /// <summary>Insert an event handle info to the cache.
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="eventHandlerTypeName"></param>
        public void AddEventHandleInfo(Guid eventId, string eventHandlerTypeName)
        {
            IList<string> handlerTypeNameList;
            if (!_handleInfoDict.TryGetValue(eventId, out handlerTypeNameList))
            {
                handlerTypeNameList = new List<string>();
            }
            handlerTypeNameList.Add(eventHandlerTypeName);
        }
        /// <summary>Check whether the given event was handled by the given event handler.
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="eventHandlerTypeName"></param>
        /// <returns></returns>
        public bool IsEventHandleInfoExist(Guid eventId, string eventHandlerTypeName)
        {
            IList<string> handlerTypeNameList;
            return _handleInfoDict.TryGetValue(eventId, out handlerTypeNameList) && handlerTypeNameList.Contains(eventHandlerTypeName);
        }
        /// <summary>Remove all the event handle information from the cache by the given eventId.
        /// </summary>
        /// <param name="eventId"></param>
        public void RemoveEventHandleInfo(Guid eventId)
        {
            IList<string> handlerTypeNameList;
            _handleInfoDict.TryRemove(eventId, out handlerTypeNameList);
        }
    }
}
