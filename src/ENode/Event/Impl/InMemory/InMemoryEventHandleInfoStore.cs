using System;
using System.Collections.Concurrent;

namespace ENode.Eventing {
    public class InMemoryEventHandleInfoStore : IEventHandleInfoStore {
        private ConcurrentDictionary<EventHandleInfo, int> _versionDict = new ConcurrentDictionary<EventHandleInfo, int>();

        public void AddEventHandleInfo(Guid eventId, string eventHandlerTypeName) {
            _versionDict.TryAdd(new EventHandleInfo(eventId, eventHandlerTypeName), 0);
        }
        public bool IsEventHandleInfoExist(Guid eventId, string eventHandlerTypeName) {
            return _versionDict.ContainsKey(new EventHandleInfo(eventId, eventHandlerTypeName));
        }

        class EventHandleInfo {
            public Guid EventId { get; private set; }
            public string EventHandlerTypeName { get; private set; }

            public EventHandleInfo(Guid eventId, string eventHandlerTypeName) {
                EventId = eventId;
                EventHandlerTypeName = eventHandlerTypeName;
            }

            public override bool Equals(object obj) {
                var another = obj as EventHandleInfo;

                if (another == null) {
                    return false;
                }
                else if (another == this) {
                    return true;
                }

                return EventId == another.EventId && EventHandlerTypeName == another.EventHandlerTypeName;
            }
            public override int GetHashCode() {
                return EventId.GetHashCode() + EventHandlerTypeName.GetHashCode();
            }
        }
    }
}
