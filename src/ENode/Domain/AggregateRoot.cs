using System;
using System.Collections.Generic;
using System.Linq;
using ENode.Eventing;
using ENode.Infrastructure;
using ENode.Snapshoting;

namespace ENode.Domain
{
    /// <summary>Abstract base aggregate root class.
    /// </summary>
    [Serializable]
    public abstract class AggregateRoot
    {
        #region Private Variables

        private Queue<IEvent> _uncommittedEvents;
        private static IAggregateRootInternalHandlerProvider _eventHandlerProvider = ObjectContainer.Resolve<IAggregateRootInternalHandlerProvider>();

        #endregion

        #region Constructurs

        /// <summary>Default constructor.
        /// </summary>
        protected AggregateRoot()
        {
            _uncommittedEvents = new Queue<IEvent>();
        }
        /// <summary>Parameterized constructor with an uniqueId.
        /// </summary>
        /// <param name="uniqueId">The string uniqueId.</param>
        protected AggregateRoot(string uniqueId) : this()
        {
            UniqueId = uniqueId;
        }

        #endregion

        #region Public Properties

        /// <summary>Represents the uniqueId of the aggregate root.
        /// </summary>
        public string UniqueId { get; protected set; }
        /// <summary>Represents the current event stream version of the aggregate root.
        /// <remarks>
        /// This version record the total event stream count of the current aggregate root, this version is always continuous.
        /// </remarks>
        /// </summary>
        public long Version { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>Used for DCI pattern support. This method will make the aggregate root act as a specified role interface.
        /// <remarks>
        /// Note: the aggregate must implement the role interface, otherwise exception will be raised.
        /// </remarks>
        /// </summary>
        /// <typeparam name="TRole">The role interface type.</typeparam>
        /// <returns>Returns the current aggregate root which its type is converted to the role interface.</returns>
        public TRole ActAs<TRole>() where TRole : class
        {
            if (!typeof(TRole).IsInterface)
            {
                throw new Exception(string.Format("TRole '{0}' must be an interface.", typeof(TRole).FullName));
            }

            var role = this as TRole;

            if (role == null)
            {
                throw new Exception(string.Format("AggregateRoot '{0}' can not act as role '{1}'.", GetType().FullName, typeof(TRole).FullName));
            }

            return role;
        }

        #endregion

        #region Protected Methods

        /// <summary>Raise a domain event.
        /// <remarks>
        /// The event first will be handled by the current aggregate root, and then be queued in the local queue of the current aggregate root.
        /// </remarks>
        /// </summary>
        /// <param name="evnt">The domain event to be raised.</param>
        protected void RaiseEvent<T>(T evnt) where T : class, IEvent
        {
            HandleEvent<T>(evnt);
            QueueEvent(evnt);
        }

        #endregion

        #region Internal Methods

        /// <summary>Get all the uncommitted events of the current aggregate root.
        /// </summary>
        internal IEnumerable<IEvent> GetUncommittedEvents()
        {
            return _uncommittedEvents;
        }
        /// <summary>Replay the given event stream.
        /// </summary>
        /// <param name="eventStream"></param>
        internal void ReplayEventStream(EventStream eventStream)
        {
            ReplayEventStreams(new EventStream[] { eventStream });
        }
        /// <summary>Replay the given event streams.
        /// </summary>
        internal void ReplayEventStreams(IEnumerable<EventStream> eventStreams)
        {
            if (_uncommittedEvents.Any())
            {
                _uncommittedEvents.Clear();
            }

            foreach (var eventStream in eventStreams)
            {
                if (eventStream.Version == 1)
                {
                    UniqueId = eventStream.AggregateRootId;
                }
                VerifyEvent(eventStream);
                ApplyEvent(eventStream);
            }
        }
        /// <summary>Initialize from the given snapshot.
        /// </summary>
        internal void InitializeFromSnapshot(Snapshot snapshot)
        {
            UniqueId = snapshot.AggregateRootId;
            Version = snapshot.Version;
            _uncommittedEvents = new Queue<IEvent>();
            if (_eventHandlerProvider == null)
            {
                _eventHandlerProvider = ObjectContainer.Resolve<IAggregateRootInternalHandlerProvider>();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>Handle the given event and update the aggregate root status.
        /// </summary>
        private void HandleEvent<T>(T evnt) where T : class, IEvent
        {
            var eventHandler = this as IEventHandler<T>;
            if (eventHandler != null)
            {
                eventHandler.Handle(evnt);
            }
            else
            {
                var handler = _eventHandlerProvider.GetInternalEventHandler(this.GetType(), evnt.GetType());
                if (handler == null)
                {
                    throw new Exception(string.Format("Event handler not found on {0} for {1}.", GetType().FullName, evnt.GetType().FullName));
                }

                handler(this, evnt);
            }
        }
        /// <summary>Verify whether the given event stream can be applied on the current aggregate root.
        /// </summary>
        private void VerifyEvent(EventStream eventStream)
        {
            if (eventStream.AggregateRootId != UniqueId)
            {
                var errorMessage = string.Format("Cannot apply event stream to aggregate root as the AggregateRootId not matched. EventStream Id:{0}, AggregateRootId:{1}; Current AggregateRootId:{2}",
                                        eventStream.Id,
                                        eventStream.AggregateRootId,
                                        UniqueId);
                throw new Exception(errorMessage);
            }

            if (eventStream.Version != Version + 1)
            {
                var errorMessage = string.Format("Cannot apply event stream to aggregate root as the version not matched. EventStream Id:{0}, Version:{1}; Current AggregateRoot Version:{2}",
                                        eventStream.Id,
                                        eventStream.Version,
                                        Version);
                throw new Exception(errorMessage);
            }
        }
        /// <summary>Apply all the events of the given event stream to the current aggregate root.
        /// </summary>
        /// <param name="eventStream"></param>
        private void ApplyEvent(EventStream eventStream)
        {
            foreach (var evnt in eventStream.Events)
            {
                HandleEvent(evnt);
            }
            Version = eventStream.Version;
        }
        /// <summary>Queue a uncommitted event into the local event queue.
        /// </summary>
        private void QueueEvent(IEvent uncommittedEvent)
        {
            if (_uncommittedEvents == null)
            {
                _uncommittedEvents = new Queue<IEvent>();
            }
            _uncommittedEvents.Enqueue(uncommittedEvent);
        }

        #endregion
    }
    /// <summary>Abstract base aggregate root class with strong type aggregate root id.
    /// </summary>
    [Serializable]
    public abstract class AggregateRoot<TAggregateRootId> : AggregateRoot
    {
        /// <summary>Default constructor.
        /// </summary>
        protected AggregateRoot() { }
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="id"></param>
        protected AggregateRoot(TAggregateRootId id) : base(id.ToString()) { }

        /// <summary>The strong type id of the aggregate root.
        /// </summary>
        public TAggregateRootId Id
        {
            get
            {
                return UniqueId != null ? Utils.ConvertType<TAggregateRootId>(UniqueId) : default(TAggregateRootId);
            }
            set
            {
                UniqueId = Utils.ConvertType<string>(value);
            }
        }
    }
}
