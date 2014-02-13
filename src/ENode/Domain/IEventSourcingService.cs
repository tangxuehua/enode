using System.Collections.Generic;
using ENode.Eventing;

namespace ENode.Domain
{
    /// <summary>Defines a service to provide the event sourcing facility.
    /// </summary>
    public interface IEventSourcingService
    {
        /// <summary>Initialize the given aggregate root.
        /// </summary>
        /// <param name="aggregateRoot"></param>
        void InitializeAggregateRoot(IAggregateRoot aggregateRoot);
        /// <summary>Replay the given event streams on the given aggregate root.
        /// </summary>
        void ReplayEvents(IAggregateRoot aggregateRoot, IEnumerable<EventStream> eventStreams);
    }
}
