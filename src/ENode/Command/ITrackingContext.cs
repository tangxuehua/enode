using System.Collections.Generic;
using ENode.Domain;

namespace ENode.Commanding
{
    /// <summary>Represents a internal tracking context for tracking aggregates withing a command context.
    /// </summary>
    public interface ITrackingContext
    {
        /// <summary>Get all the tracked aggregates.
        /// </summary>
        /// <returns></returns>
        IEnumerable<AggregateRoot> GetTrackedAggregateRoots();
        /// <summary>Clear all the tracked aggregates.
        /// </summary>
        void Clear();
    }
}
