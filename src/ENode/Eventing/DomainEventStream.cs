using System;
using System.Collections.Generic;
using System.Linq;
using ENode.Messaging;

namespace ENode.Eventing
{
    [Serializable]
    public class DomainEventStream : Message
    {
        public DomainEventStream(string commandId, string aggregateRootId, string aggregateRootTypeName, DateTime timestamp, IEnumerable<IDomainEvent> events, IDictionary<string, string> items = null) : base()
        {
            if (events == null || events.Count() == 0)
            {
                throw new ArgumentException("Parameter events cannot be null or empty.");
            }
            CommandId = commandId;
            AggregateRootId = aggregateRootId;
            AggregateRootTypeName = aggregateRootTypeName;
            Version = events.First().Version;
            Timestamp = timestamp;
            Events = events;
            Items = items ?? new Dictionary<string, string>();
            Id = aggregateRootId + "_" + Version;
            var sequence = 1;
            foreach (var evnt in Events)
            {
                if (evnt.AggregateRootStringId != aggregateRootId)
                {
                    throw new Exception(string.Format("Invalid domain event aggregateRootId, aggregateRootTypeName: {0} expected aggregateRootId: {1}, but was: {2}",
                        AggregateRootTypeName,
                        AggregateRootId,
                        evnt.AggregateRootStringId));
                }
                if (evnt.Version != Version)
                {
                    throw new Exception(string.Format("Invalid domain event version, aggregateRootTypeName: {0} aggregateRootId: {1} expected version: {2}, but was: {3}",
                        AggregateRootTypeName,
                        AggregateRootId,
                        Version,
                        evnt.Version));
                }
                evnt.CommandId = commandId;
                evnt.AggregateRootTypeName = aggregateRootTypeName;
                evnt.Sequence = sequence++;
                evnt.Timestamp = timestamp;
                evnt.MergeItems(Items);
            }
        }

        public string CommandId { get; private set; }
        public string AggregateRootTypeName { get; private set; }
        public string AggregateRootId { get; private set; }
        public int Version { get; private set; }
        public IEnumerable<IDomainEvent> Events { get; private set; }

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
