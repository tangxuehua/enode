using System.Collections.Generic;
using ENode.Domain;

namespace ENode.Commanding
{
    /// <summary>Represents a tracking context for tracking changed aggregate roots in the command handler.
    /// </summary>
    public interface ITrackingContext
    {
        /// <summary>Get all the tracked aggregates.
        /// </summary>
        /// <returns></returns>
        IEnumerable<IAggregateRoot> GetTrackedAggregateRoots();
    }
}
