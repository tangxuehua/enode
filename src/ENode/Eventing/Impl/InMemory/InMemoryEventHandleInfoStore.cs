using System;
using System.Collections.Concurrent;

namespace ENode.Eventing.Impl.InMemory
{
    /// <summary>Local in-memory implementation of IEventHandleInfoStore using ConcurrentDictionary.
    /// </summary>
    public class InMemoryEventHandleInfoStore : IEventHandleInfoStore
    {
        private readonly ConcurrentDictionary<EventHandleInfo, int> _versionDict = new ConcurrentDictionary<EventHandleInfo, int>();

        /// <summary>Insert an event handle info.
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="eventHandlerTypeName"></param>
        public void AddEventHandleInfo(Guid eventId, string eventHandlerTypeName)
        {
            _versionDict.TryAdd(new EventHandleInfo(eventId, eventHandlerTypeName), 0);
        }
        /// <summary>Check whether the given event was handled by the given event handler.
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="eventHandlerTypeName"></param>
        /// <returns></returns>
        public bool IsEventHandleInfoExist(Guid eventId, string eventHandlerTypeName)
        {
            return _versionDict.ContainsKey(new EventHandleInfo(eventId, eventHandlerTypeName));
        }

        class EventHandleInfo
        {
            private Guid EventId { get; set; }
            private string EventHandlerTypeName { get; set; }

            public EventHandleInfo(Guid eventId, string eventHandlerTypeName)
            {
                EventId = eventId;
                EventHandlerTypeName = eventHandlerTypeName;
            }

            public override bool Equals(object obj)
            {
                var another = obj as EventHandleInfo;

                if (another == null)
                {
                    return false;
                }
                if (another == this)
                {
                    return true;
                }

                return EventId == another.EventId && EventHandlerTypeName == another.EventHandlerTypeName;
            }
            public override int GetHashCode()
            {
                return EventId.GetHashCode() + EventHandlerTypeName.GetHashCode();
            }
        }
    }
}
