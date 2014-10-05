using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ENode.Eventing.Impl.InMemory
{
    public class InMemoryEventHandleInfoCache : IEventHandleInfoCache
    {
        private readonly ConcurrentDictionary<string, IList<int>> _handleInfoDict = new ConcurrentDictionary<string, IList<int>>();

        public void AddEventHandleInfo(string eventId, int eventHandlerTypeCode, int eventTypeCode, string aggregateRootId, int aggregateRootVersion)
        {
            IList<int> handlerTypeCodeList;
            if (!_handleInfoDict.TryGetValue(eventId, out handlerTypeCodeList))
            {
                handlerTypeCodeList = new List<int>();
            }
            handlerTypeCodeList.Add(eventHandlerTypeCode);
        }
        public bool IsEventHandleInfoExist(string eventId, int eventHandlerTypeCode)
        {
            IList<int> handlerTypeCodeList;
            return _handleInfoDict.TryGetValue(eventId, out handlerTypeCodeList) && handlerTypeCodeList.Contains(eventHandlerTypeCode);
        }
        public void RemoveEventHandleInfo(string eventId)
        {
            IList<int> handlerTypeCodeList;
            _handleInfoDict.TryRemove(eventId, out handlerTypeCodeList);
        }
    }
}
