using System;

namespace ENode.Eventing
{
    /// <summary>Represents a in-memory cache to store the event handle information of aggregate.
    /// </summary>
    public interface IEventHandleInfoCache
    {
        /// <summary>Add an event handle info to the cache.
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="eventHandlerTypeName"></param>
        void AddEventHandleInfo(Guid eventId, string eventHandlerTypeName);
        /// <summary>Check whether the given event was handled by the given event handler.
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="eventHandlerTypeName"></param>
        /// <returns></returns>
        bool IsEventHandleInfoExist(Guid eventId, string eventHandlerTypeName);
        /// <summary>Remove all the event handle information from the cache by the given eventId.
        /// </summary>
        /// <param name="eventId"></param>
        void RemoveEventHandleInfo(Guid eventId);
    }
}
