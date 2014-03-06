using System;

namespace ENode.Eventing
{
    /// <summary>Represents a storage to store the event handle information of aggregate.
    /// </summary>
    public interface IEventHandleInfoStore
    {
        /// <summary>Add an event handle info.
        /// </summary>
        void AddEventHandleInfo(string eventId, int eventHandlerTypeCode);
        /// <summary>Check whether the given event was handled by the given event handler.
        /// </summary>
        bool IsEventHandleInfoExist(string eventId, int eventHandlerTypeCode);
    }
}
