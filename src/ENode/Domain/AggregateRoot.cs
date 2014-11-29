using System;
using System.Collections.Generic;
using ECommon.Components;
using ENode.Eventing;

namespace ENode.Domain
{
    /// <summary>Represents an abstract base aggregate root.
    /// </summary>
    /// <typeparam name="TAggregateRootId"></typeparam>
    [Serializable]
    public abstract class AggregateRoot<TAggregateRootId> : IAggregateRoot
    {
        private static IAggregateRootInternalHandlerProvider _eventHandlerProvider;
        private int _version;
        private Queue<IDomainEvent> _uncommittedEvents;
        protected TAggregateRootId _id;

        /// <summary>Represents the unique identifier of the aggregate root.
        /// </summary>
        public TAggregateRootId Id
        {
            get { return _id; }
        }

        /// <summary>Parameterized constructor.
        /// </summary>
        protected AggregateRoot(TAggregateRootId id)
        {
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }
            _id = id;
            _uncommittedEvents = new Queue<IDomainEvent>();
        }

        /// <summary>Act the current aggregate as the given type of role.
        /// <remarks>
        /// Rhe current aggregate must implement the role interface, otherwise this method will throw exception.
        /// </remarks>
        /// </summary>
        /// <typeparam name="TRole">The role interface type.</typeparam>
        /// <returns>Returns the role instance which is acted by the current aggregate.</returns>
        public TRole ActAs<TRole>() where TRole : class
        {
            if (!typeof(TRole).IsInterface)
            {
                throw new Exception(string.Format("'{0}' is not an interface type.", typeof(TRole).Name));
            }

            var actor = this as TRole;

            if (actor == null)
            {
                throw new Exception(string.Format("'{0}' can not act as role '{1}'.", GetType().FullName, typeof(TRole).Name));
            }

            return actor;
        }
        /// <summary>Apply a domain event to the current aggregate root.
        /// </summary>
        /// <param name="domainEvent"></param>
        protected void ApplyEvent(IDomainEvent domainEvent)
        {
            HandleEvent(domainEvent);
            AppendUncommittedEvent(domainEvent);
        }

        private void HandleEvent(IDomainEvent domainEvent)
        {
            if (_eventHandlerProvider == null)
            {
                _eventHandlerProvider = ObjectContainer.Resolve<IAggregateRootInternalHandlerProvider>();
            }
            var handler = _eventHandlerProvider.GetInternalEventHandler(GetType(), domainEvent.GetType());
            if (handler == null)
            {
                throw new Exception(string.Format("Could not find event handler for [{0}] of [{1}]", domainEvent.GetType().FullName, GetType().FullName));
            }
            handler(this, domainEvent);
        }
        private void AppendUncommittedEvent(IDomainEvent domainEvent)
        {
            if (_uncommittedEvents == null)
            {
                _uncommittedEvents = new Queue<IDomainEvent>();
            }
            _uncommittedEvents.Enqueue(domainEvent);
        }
        private void VerifyEvent(DomainEventStream eventStream)
        {
            var current = this as IAggregateRoot;
            if (eventStream.Version > 1 && eventStream.AggregateRootId != current.UniqueId)
            {
                throw new Exception(string.Format("Invalid domain event stream, aggregateRootId:{0}, expected aggregateRootId:{1}", eventStream.AggregateRootId, current.UniqueId));
            }
            if (eventStream.Version != current.Version + 1)
            {
                throw new Exception(string.Format("Invalid domain event stream, version:{0}, expected version:{1}", eventStream.Version, current.Version));
            }
        }

        string IAggregateRoot.UniqueId
        {
            get
            {
                if (Id != null)
                {
                    return Id.ToString();
                }
                return null;
            }
        }
        int IAggregateRoot.Version
        {
            get { return _version; }
        }
        IEnumerable<IDomainEvent> IAggregateRoot.GetChanges()
        {
            if (_uncommittedEvents == null)
            {
                return new IDomainEvent[0];
            }
            return _uncommittedEvents.ToArray();
        }
        void IAggregateRoot.AcceptChanges(int newVersion)
        {
            if (_version + 1 != newVersion)
            {
                throw new Exception(string.Format("Cannot accept invalid version: {0}, expect version: {1}", newVersion, _version + 1));
            }
            _version = newVersion;
            _uncommittedEvents.Clear();
        }
        void IAggregateRoot.ReplayEvents(IEnumerable<DomainEventStream> eventStreams)
        {
            if (eventStreams == null) return;

            foreach (var eventStream in eventStreams)
            {
                VerifyEvent(eventStream);
                foreach (var domainEvent in eventStream.DomainEvents)
                {
                    HandleEvent(domainEvent);
                }
                _version = eventStream.Version;
            }
        }
    }
}
