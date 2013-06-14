using System;
using ENode.Eventing;

namespace ENode.Domain
{
    /// <summary>Represents a service to refresh memory cache.
    /// </summary>
    public interface IMemoryCacheRefreshService
    {
        /// <summary>Refresh the given event stream into memory cache.
        /// </summary>
        void Refresh(EventStream stream);
        /// <summary>Refresh the memory cache for the given aggregate root.
        /// </summary>
        void Refresh(Type aggregateRootType, string aggregateRootId);
    }
}
