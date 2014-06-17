using System;
using System.Collections.Generic;
using System.Linq;
using ENode.Infrastructure;

namespace ENode.Eventing
{
    /// <summary>Represents a stream of domain event.
    /// <remarks>
    /// One stream may contains several domain events, but they must belong to a single aggregate.
    /// </remarks>
    /// </summary>
    [Serializable]
    public class EventStream
    {
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="commitId"></param>
        /// <param name="aggregateRootId"></param>
        /// <param name="aggregateRootTypeCode"></param>
        /// <param name="processId"></param>
        /// <param name="version"></param>
        /// <param name="timestamp"></param>
        /// <param name="events"></param>
        public EventStream(string commitId, string aggregateRootId, int aggregateRootTypeCode, string processId, int version, DateTime timestamp, IEnumerable<IDomainEvent> events)
        {
            CommitId = commitId;
            AggregateRootId = aggregateRootId;
            AggregateRootTypeCode = aggregateRootTypeCode;
            ProcessId = processId;
            Version = version;
            Timestamp = timestamp;
            VerifyEvents(events);
            Events = events;
        }

        /// <summary>The commandId which generate this event stream.
        /// </summary>
        public string CommitId { get; private set; }
        /// <summary>The aggregate root type code.
        /// </summary>
        public int AggregateRootTypeCode { get; private set; }
        /// <summary>The aggregate root id.
        /// </summary>
        public string AggregateRootId { get; private set; }
        /// <summary>The version of the event stream.
        /// </summary>
        public int Version { get; private set; }
        /// <summary>The process id which the current event associated.
        /// </summary>
        public string ProcessId { get; private set; }
        /// <summary>The occurred time of the event stream.
        /// </summary>
        public DateTime Timestamp { get; private set; }
        /// <summary>The domain events of the event stream.
        /// </summary>
        public IEnumerable<IDomainEvent> Events { get; private set; }

        /// <summary>Overrides to return the whole event stream information.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var format = "[CommitId={0},AggregateRootTypeCode={1},AggregateRootId={2},Version={3},ProcessId={4},Timestamp={5},Events={6}]";
            return string.Format(format, CommitId, AggregateRootTypeCode, AggregateRootId, Version, ProcessId, Timestamp, string.Join("|", Events.Select(x => x.GetType().Name)));
        }

        private void VerifyEvents(IEnumerable<IDomainEvent> events)
        {
            if (events.Count() == 0)
            {
                throw new ENodeException("Events cannot be empty.");
            }
            foreach (var evnt in events)
            {
                if (evnt.AggregateRootId != AggregateRootId)
                {
                    throw new ENodeException("Domain event aggregate root Id mismatch, current domain event aggregateRootId:{0}, expected aggregateRootId:{1}", evnt.AggregateRootId, AggregateRootId);
                }
                if (evnt.Version != Version)
                {
                    throw new ENodeException("Domain event version mismatch, current domain event version:{0}, expected version:{1}", evnt.Version, Version);
                }
            }
        }
    }
}
