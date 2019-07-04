using System;
using System.Collections.Generic;
using System.Linq;
using ECommon.Utilities;

namespace ENode.Eventing
{
    [Serializable]
    public class DomainEventStream
    {
        public DomainEventStream(string commandId, string aggregateRootId, string aggregateRootTypeName, int version, DateTime timestamp, IEnumerable<IDomainEvent> events, IDictionary<string, string> items = null)
        {
            Id = ObjectId.GenerateNewStringId();
            CommandId = commandId;
            AggregateRootId = aggregateRootId;
            AggregateRootTypeName = aggregateRootTypeName;
            Version = version;
            Timestamp = timestamp;
            Events = events;
            Items = items ?? new Dictionary<string, string>();
            var sequence = 1;
            foreach (var evnt in Events)
            {
                if (evnt.Version != this.Version)
                {
                    throw new Exception(string.Format("Invalid domain event version, aggregateRootTypeName: {0} aggregateRootId: {1} expected version: {2}, but was: {3}",
                        this.AggregateRootTypeName,
                        this.AggregateRootId,
                        this.Version,
                        evnt.Version));
                }
                evnt.AggregateRootTypeName = aggregateRootTypeName;
                evnt.Sequence = sequence++;
            }
        }

        public string Id { get; private set; }
        public string CommandId { get; private set; }
        public string AggregateRootTypeName { get; private set; }
        public string AggregateRootId { get; private set; }
        public int Version { get; private set; }
        public IEnumerable<IDomainEvent> Events { get; private set; }
        public DateTime Timestamp { get; private set; }
        public IDictionary<string, string> Items { get; internal set; }

        public override string ToString()
        {
            var format = "[Id={0},CommandId={1},AggregateRootTypeName={2},AggregateRootId={3},Version={4},Timestamp={5},Events={6},Items={7}]";
            return string.Format(format,
                Id,
                CommandId,
                AggregateRootTypeName,
                AggregateRootId,
                Version,
                Timestamp,
                string.Join("|", Events.Select(x => x.GetType().Name)),
                string.Join("|", Items.Select(x => x.Key + ":" + x.Value)));
        }
    }
}
