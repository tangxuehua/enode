using System;
using System.Collections.Generic;

namespace ENode.Eventing
{
    /// <summary>Represents a event store to store event streams of aggregate.
    /// </summary>
    public interface IEventStore
    {
        /// <summary>Commit the event stream to the event store.
        /// </summary>
        EventCommitStatus Commit(EventStream stream);
        /// <summary>Query event streams from event store.
        /// </summary>
        IEnumerable<EventStream> Query(object aggregateRootId, string aggregateRootName, long minStreamVersion, long maxStreamVersion);
        /// <summary>Query all the event streams from the event store.
        /// </summary>
        /// <returns></returns>
        IEnumerable<EventStream> QueryAll();
    }
}
