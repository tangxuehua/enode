using System.Collections.Generic;

namespace ENode.Eventing
{
    /// <summary>Represents a event store to store event commit records of aggregate.
    /// </summary>
    public interface IEventStore
    {
        /// <summary>Append the given event stream to the event store.
        /// </summary>
        EventAppendResult Append(EventStream eventStream);
        /// <summary>Find a single event stream by aggregateRootId and version.
        /// </summary>
        /// <returns></returns>
        EventStream Find(string aggregateRootId, int version);
        /// <summary>Find a single event stream by aggregateRootId and commitId.
        /// </summary>
        /// <returns></returns>
        EventStream Find(string aggregateRootId, string commitId);
        /// <summary>Query a range of event streams of a single aggregate from event store.
        /// </summary>
        IEnumerable<EventStream> QueryAggregateEvents(string aggregateRootId, int aggregateRootTypeCode, int minVersion, int maxVersion);
        /// <summary>Query a range of event streams from event store by page.
        /// </summary>
        /// <returns></returns>
        IEnumerable<EventStream> QueryByPage(int pageIndex, int pageSize);
    }
}
