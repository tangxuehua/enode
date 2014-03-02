using System;
using System.Collections.Generic;

namespace ENode.Eventing
{
    /// <summary>Represents a event store to store event streams of aggregate.
    /// </summary>
    public interface IEventStore
    {
        /// <summary>Represents whether the event store available.
        /// </summary>
        bool IsAvailable { get; }
        /// <summary>Initialize the event store.
        /// </summary>
        void Initialize();
        /// <summary>Get the event stream by aggregateRootId and commandId.
        /// </summary>
        /// <returns></returns>
        EventStream GetEventStream(string aggregateRootId, Guid commandId);
        /// <summary>Commit the event stream to the event store.
        /// </summary>
        EventCommitStatus Commit(EventStream stream);
        /// <summary>Query event streams from event store.
        /// </summary>
        IEnumerable<EventStream> Query(string aggregateRootId, string aggregateRootName, long minStreamVersion, long maxStreamVersion);
        /// <summary>Query all the event streams from the event store.
        /// </summary>
        /// <returns></returns>
        IEnumerable<EventStream> QueryAll();
    }
}
