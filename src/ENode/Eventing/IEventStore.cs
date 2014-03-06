using System.Collections.Generic;

namespace ENode.Eventing
{
    /// <summary>Represents a event store to store event commit records of aggregate.
    /// </summary>
    public interface IEventStore
    {
        /// <summary>Append the given event commit record to the event store.
        /// </summary>
        EventAppendResult Append(EventCommitRecord record);
        /// <summary>Find a single event commit record by aggregateRootId and commitId.
        /// </summary>
        /// <returns></returns>
        EventCommitRecord Find(string aggregateRootId, string commitId);
        /// <summary>Query a range of event commit records of a single aggregate from event store.
        /// </summary>
        IEnumerable<EventCommitRecord> QueryAggregateEvents(string aggregateRootId, int aggregateRootTypeCode, int minVersion, int maxVersion);
        /// <summary>Query a range of event commit records from event store by page.
        /// </summary>
        /// <returns></returns>
        IEnumerable<EventCommitRecord> QueryByPage(int pageIndex, int pageSize);
    }
}
