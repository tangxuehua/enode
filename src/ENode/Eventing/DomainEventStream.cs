using System;
using System.Collections.Generic;
using System.Linq;
using ENode.Infrastructure;

namespace ENode.Eventing
{
    [Serializable]
    public class DomainEventStream : EventStream, IDomainEventStream
    {
        public DomainEventStream(string commandId, string aggregateRootId, int aggregateRootTypeCode, string processId, int version, DateTime timestamp, IEnumerable<IDomainEvent> events, IDictionary<string, string> items)
            : base(commandId, processId, events, items)
        {
            AggregateRootId = aggregateRootId;
            AggregateRootTypeCode = aggregateRootTypeCode;
            Version = version;
            Timestamp = timestamp;
            VerifyEvents(events);
            DomainEvents = events;
        }

        public int AggregateRootTypeCode { get; private set; }
        public string AggregateRootId { get; private set; }
        public int Version { get; private set; }
        public DateTime Timestamp { get; private set; }
        public IEnumerable<IDomainEvent> DomainEvents { get; private set; }

        public override string ToString()
        {
            var format = "[CommandId={0},AggregateRootTypeCode={1},AggregateRootId={2},Version={3},ProcessId={4},Timestamp={5},DomainEvents={6},Items={7}]";
            return string.Format(format,
                CommandId,
                AggregateRootTypeCode,
                AggregateRootId,
                Version,
                ProcessId,
                Timestamp,
                string.Join("|", DomainEvents.Select(x => x.GetType().Name)),
                string.Join("|", Items.Select(x => x.Key + ":" + x.Value)));
        }

        private void VerifyEvents(IEnumerable<IDomainEvent> events)
        {
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
