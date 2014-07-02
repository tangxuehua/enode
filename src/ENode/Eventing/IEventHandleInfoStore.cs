using System;

namespace ENode.Eventing
{
    /// <summary>Represents a storage to store the event handle information of aggregate.
    /// </summary>
    public interface IEventHandleInfoStore
    {
        /// <summary>Add an event handle info.
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="eventHandlerTypeCode"></param>
        /// <param name="eventTypeCode"></param>
        /// <param name="aggregateRootId"></param>
        /// <param name="aggregateRootVersion"></param>
        void AddEventHandleInfo(string eventId, int eventHandlerTypeCode, int eventTypeCode, string aggregateRootId, int aggregateRootVersion);
        /// <summary>Check whether the given event was handled by the given event handler.
        /// </summary>
        bool IsEventHandleInfoExist(string eventId, int eventHandlerTypeCode);
    }
}
