
namespace ENode.Eventing
{
    /// <summary>Represents a in-memory cache to store the event handle information of aggregate.
    /// </summary>
    public interface IEventHandleInfoCache
    {
        /// <summary>Add an event handle info to the cache.
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="eventHandlerTypeCode"></param>
        /// <param name="eventTypeCode"></param>
        /// <param name="aggregateRootId"></param>
        /// <param name="aggregateRootVersion"></param>
        void AddEventHandleInfo(string eventId, int eventHandlerTypeCode, int eventTypeCode, string aggregateRootId, int aggregateRootVersion);
        /// <summary>Check whether the given event was handled by the given event handler.
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="eventHandlerTypeCode"></param>
        /// <returns></returns>
        bool IsEventHandleInfoExist(string eventId, int eventHandlerTypeCode);
        /// <summary>Remove all the event handle information from the cache by the given eventId.
        /// </summary>
        /// <param name="eventId"></param>
        void RemoveEventHandleInfo(string eventId);
    }
}
