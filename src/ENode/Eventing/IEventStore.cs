using System.Collections.Generic;

namespace ENode.Eventing
{
    /// <summary>Represents a event store to store event commit records of aggregate.
    /// </summary>
    public interface IEventStore
    {
        /// <summary>Batch append the given event streams to the event store.
        /// </summary>
        EventAppendResult BatchAppend(IEnumerable<DomainEventStream> eventStreams);
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
    }
}
