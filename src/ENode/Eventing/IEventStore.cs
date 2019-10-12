using System.Collections.Generic;
using System.Threading.Tasks;

namespace ENode.Eventing
{
    /// <summary>Represents a event store to store event commit records of aggregate.
    /// </summary>
    public interface IEventStore
    {
        /// <summary>Batch append the given event streams to the event store async.
        /// </summary>
        Task<EventAppendResult> BatchAppendAsync(IEnumerable<DomainEventStream> eventStreams);
        /// <summary>Find a single event stream by aggregateRootId and version async.
        /// </summary>
        /// <returns></returns>
        Task<DomainEventStream> FindAsync(string aggregateRootId, int version);
        /// <summary>Find a single event stream by aggregateRootId and commandId async.
        /// </summary>
        /// <returns></returns>
        Task<DomainEventStream> FindAsync(string aggregateRootId, string commandId);
        /// <summary>Query a range of event streams of a single aggregate from event store async.
        /// </summary>
        Task<IEnumerable<DomainEventStream>> QueryAggregateEventsAsync(string aggregateRootId, string aggregateRootTypeName, int minVersion, int maxVersion);
    }
}
