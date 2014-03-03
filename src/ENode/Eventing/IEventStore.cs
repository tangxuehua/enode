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
        /// <summary>Get the event byte stream by aggregateRootId and commitId.
        /// </summary>
        /// <returns></returns>
        EventByteStream GetEventStream(string aggregateRootId, string commitId);
        /// <summary>Commit the event byte stream to the event store.
        /// </summary>
        EventCommitStatus Commit(EventByteStream stream);
        /// <summary>Query event byte streams from event store.
        /// </summary>
        IEnumerable<EventByteStream> Query(string aggregateRootId, string aggregateRootName, int minStreamVersion, int maxStreamVersion);
        /// <summary>Query all the event byte streams from the event store.
        /// </summary>
        /// <returns></returns>
        IEnumerable<EventByteStream> QueryAll();
    }
}
