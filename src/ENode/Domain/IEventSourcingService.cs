using System;
using System.Collections.Generic;
using ENode.Eventing;

namespace ENode.Domain
{
    /// <summary>Defines a service to provide the event sourcing facility.
    /// </summary>
    public interface IEventSourcingService
    {
        /// <summary>Replay the given event streams on the given aggregate root.
        /// </summary>
        void ReplayEventStream(IAggregateRoot aggregateRoot, IEnumerable<EventStream> eventStreams);
    }
}
