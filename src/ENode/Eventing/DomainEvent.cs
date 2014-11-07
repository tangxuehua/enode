using System;

namespace ENode.Eventing
{
    /// <summary>Represents an abstract base domain event.
    /// </summary>
    [Serializable]
    public abstract class DomainEvent<TAggregateRootId> : Event, IDomainEvent
    {
        private string _aggregateRootId;
        private int? _version;

        /// <summary>Parameterized constructor.
        /// </summary>
        public DomainEvent(TAggregateRootId aggregateRootId) : base()
        {
            if (aggregateRootId == null)
            {
                throw new ArgumentNullException("aggregateRootId");
            }
            AggregateRootId = aggregateRootId;
            _aggregateRootId = aggregateRootId.ToString();
        }

        /// <summary>Represents the aggregate root id of the domain event.
        /// </summary>
        public TAggregateRootId AggregateRootId { get; private set; }
        /// <summary>Represents the version of the domain event.
        /// </summary>
        public int Version
        {
            get
            {
                return _version == null ? -1 : _version.Value;
            }
        }
        /// <summary>Represents the occurred time of the domain event.
        /// </summary>
        public DateTime Timestamp { get; private set; }

        string IDomainEvent.AggregateRootId
        {
            get
            {
                if (_aggregateRootId == null && AggregateRootId != null)
                {
                    _aggregateRootId = AggregateRootId.ToString();
                }
                return _aggregateRootId;
            }
        }
        int IDomainEvent.Version
        {
            get
            {
                return Version;
            }
            set
            {
                _version = value;
            }
        }
        DateTime IDomainEvent.Timestamp
        {
            get
            {
                return this.Timestamp;
            }
            set
            {
                Timestamp = value;
            }
        }
    }
}
