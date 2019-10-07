using System;
using ENode.Infrastructure;
using ENode.Messaging;

namespace ENode.Eventing
{
    /// <summary>Represents an abstract generic domain event.
    /// </summary>
    [Serializable]
    public abstract class DomainEvent<TAggregateRootId> : Message, IDomainEvent<TAggregateRootId>
    {
        private TAggregateRootId _aggregateRootId;

        public string CommandId { get; set; }
        public TAggregateRootId AggregateRootId
        {
            get { return _aggregateRootId; }
            set
            {
                _aggregateRootId = value;
                AggregateRootStringId = value?.ToString();
            }
        }
        public string AggregateRootStringId { get; set; }
        public string AggregateRootTypeName { get; set; }
        public int Version { get; set; }
        public int SpecVersion { get; set; }
        public int Sequence { get; set; }

        public DomainEvent() : base()
        {
            Version = 1;
            SpecVersion = 1;
            Sequence = 1;
        }
    }
}
