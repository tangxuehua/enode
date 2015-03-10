using System.Collections.Generic;
using ENode.Domain;

namespace ENode.Commanding
{
    /// <summary>Represents a tracking context for tracking the changed aggregate roots for the aggregate command handler.
    /// </summary>
    public interface ITrackingContext
    {
        /// <summary>Get all the tracked aggregates.
        /// </summary>
        /// <returns></returns>
        IEnumerable<IAggregateRoot> GetTrackedAggregateRoots();
        /// <summary>Clear the tracking context.
        /// </summary>
        void Clear();
    }
}
