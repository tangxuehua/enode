using System;
using System.Collections.Generic;
using System.Linq;

namespace ENode.Eventing
{
    [Serializable]
    public class DomainEventStream : EventStream, IDomainEventStream
    {
        public DomainEventStream(string commandId, string aggregateRootId, int aggregateRootTypeCode, int version, DateTime timestamp, IEnumerable<IDomainEvent> events, IDictionary<string, string> items)
            : base(commandId, events, items)
        {
            AggregateRootId = aggregateRootId;
            AggregateRootTypeCode = aggregateRootTypeCode;
            Version = version;
            Timestamp = timestamp;
            DomainEvents = events;
            InitEvents();
        }

        public int AggregateRootTypeCode { get; private set; }
        public string AggregateRootId { get; private set; }
        public int Version { get; private set; }
        public DateTime Timestamp { get; private set; }
        public IEnumerable<IDomainEvent> DomainEvents { get; private set; }

        public override string ToString()
        {
            var format = "[CommandId={0},AggregateRootTypeCode={1},AggregateRootId={2},Version={3},Timestamp={4},DomainEvents={5},Items={6}]";
            return string.Format(format,
                CommandId,
                AggregateRootTypeCode,
                AggregateRootId,
                Version,
                Timestamp,
                string.Join("|", DomainEvents.Select(x => x.GetType().Name)),
                string.Join("|", Items.Select(x => x.Key + ":" + x.Value)));
        }

        private void InitEvents()
        {
            foreach (var domainEvent in DomainEvents)
            {
                if (domainEvent.AggregateRootId != AggregateRootId)
                {
                    throw new Exception(string.Format("Domain event aggregate root Id mismatch, current domain event aggregateRootId:{0}, expected aggregateRootId:{1}", domainEvent.AggregateRootId, AggregateRootId));
                }
                domainEvent.Version = Version;
                domainEvent.Timestamp = Timestamp;
            }
        }
    }
}
