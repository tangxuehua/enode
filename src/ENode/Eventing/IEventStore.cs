using System.Collections.Generic;
using System.Threading.Tasks;
using ENode.Infrastructure;

namespace ENode.Eventing
{
    /// <summary>Represents a event store to store event commit records of aggregate.
    /// </summary>
    public interface IEventStore
    {
        /// <summary>Batch append the given event streams to the event store.
        /// </summary>
        void BatchAppend(IEnumerable<DomainEventStream> eventStreams);
        /// <summary>Append the given event stream to the event store.
        /// </summary>
        EventAppendResult Append(DomainEventStream eventStream);
        /// <summary>Find a single event stream by aggregateRootId and version.
        /// </summary>
        /// <returns></returns>
        DomainEventStream Find(string aggregateRootId, int version);
        /// <summary>Find a single event stream by aggregateRootId and commandId.
        /// </summary>
        /// <returns></returns>
        DomainEventStream Find(string aggregateRootId, string commandId);
        /// <summary>Query a range of event streams of a single aggregate from event store.
        /// </summary>
        IEnumerable<DomainEventStream> QueryAggregateEvents(string aggregateRootId, int aggregateRootTypeCode, int minVersion, int maxVersion);
        /// <summary>Query a range of event streams from event store by page.
        /// </summary>
        /// <returns></returns>
        IEnumerable<DomainEventStream> QueryByPage(int pageIndex, int pageSize);

        /// <summary>Batch append the given event streams to the event store async.
        /// </summary>
        Task<AsyncOperationResult> BatchAppendAsync(IEnumerable<DomainEventStream> eventStreams);
        /// <summary>Append the given event stream to the event store async.
        /// </summary>
        Task<AsyncOperationResult<EventAppendResult>> AppendAsync(DomainEventStream eventStream);
        /// <summary>Find a single event stream by aggregateRootId and version async.
        /// </summary>
        /// <returns></returns>
        Task<AsyncOperationResult<DomainEventStream>> FindAsync(string aggregateRootId, int version);
        /// <summary>Find a single event stream by aggregateRootId and commandId async.
        /// </summary>
        /// <returns></returns>
        Task<AsyncOperationResult<DomainEventStream>> FindAsync(string aggregateRootId, string commandId);
        /// <summary>Query a range of event streams of a single aggregate from event store async.
        /// </summary>
        Task<AsyncOperationResult<IEnumerable<DomainEventStream>>> QueryAggregateEventsAsync(string aggregateRootId, int aggregateRootTypeCode, int minVersion, int maxVersion);
        /// <summary>Query a range of event streams from event store by page async.
        /// </summary>
        /// <returns></returns>
        Task<AsyncOperationResult<IEnumerable<DomainEventStream>>> QueryByPageAsync(int pageIndex, int pageSize);
    }
}
