using System.Collections.Generic;
using System.Threading.Tasks;
using ECommon.IO;

namespace ENode.Eventing
{
    /// <summary>Represents a event store to store event commit records of aggregate.
    /// </summary>
    public interface IEventStore
    {
        /// <summary>This property indicate whether the event store support batch append events.
        /// </summary>
        bool SupportBatchAppendEvent { get; set; }
        /// <summary>Query a range of event streams of a single aggregate from event store.
        /// </summary>
        IEnumerable<DomainEventStream> QueryAggregateEvents(string aggregateRootId, string aggregateRootTypeName, int minVersion, int maxVersion);
        /// <summary>Batch append the given event streams to the event store async.
        /// </summary>
        Task<AsyncTaskResult<EventAppendResult>> BatchAppendAsync(IEnumerable<DomainEventStream> eventStreams);
        /// <summary>Append the given event stream to the event store async.
        /// </summary>
        Task<AsyncTaskResult<EventAppendResult>> AppendAsync(DomainEventStream eventStream);
        /// <summary>Find a single event stream by aggregateRootId and version async.
        /// </summary>
        /// <returns></returns>
        Task<AsyncTaskResult<DomainEventStream>> FindAsync(string aggregateRootId, int version);
        /// <summary>Find a single event stream by aggregateRootId and commandId async.
        /// </summary>
        /// <returns></returns>
        Task<AsyncTaskResult<DomainEventStream>> FindAsync(string aggregateRootId, string commandId);
        /// <summary>Query a range of event streams of a single aggregate from event store async.
        /// </summary>
        Task<AsyncTaskResult<IEnumerable<DomainEventStream>>> QueryAggregateEventsAsync(string aggregateRootId, string aggregateRootTypeName, int minVersion, int maxVersion);
    }
}
