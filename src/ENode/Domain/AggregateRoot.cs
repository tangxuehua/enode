using System;
using System.Collections.Generic;
using System.Linq;
using ENode.Eventing;
using ENode.Infrastructure;
using ENode.Snapshoting;

namespace ENode.Domain
{
    /// <summary>Abstract aggregate root base class.
    /// </summary>
    [Serializable]
    public abstract class AggregateRoot
    {
        #region Private Variables

        private Queue<IEvent> _uncommittedEvents;
        private static IAggregateRootInternalHandlerProvider _eventHandlerProvider = ObjectContainer.Resolve<IAggregateRootInternalHandlerProvider>();

        #endregion

        #region Constructurs

        /// <summary>Default constructor
        /// </summary>
        protected AggregateRoot()
        {
            _uncommittedEvents = new Queue<IEvent>();
        }
        /// <summary>Parameterized constructor with a aggregate uniqueId.
        /// </summary>
        /// <param name="uniqueId">the aggregate uniqueId</param>
        protected AggregateRoot(string uniqueId) : this()
        {
            UniqueId = uniqueId;
        }

        #endregion

        #region Public Properties

        /// <summary>Represents the uniqueId of the aggregate.
        /// </summary>
        public string UniqueId { get; protected set; }
        /// <summary>Represents the current event stream version of the aggregate.
        /// <remarks>
        /// This version record the total event stream count of the current aggregate, this version is always continuous.
        /// </remarks>
        /// </summary>
        public long Version { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>Support the aggregate to act as a specified role, and the role must be an interface.
        /// <remarks>
        /// Note: the aggregate must implement the role interace, otherwise exception will be raised.
        /// </remarks>
        /// </summary>
        /// <typeparam name="TRole">The role interface.</typeparam>
        /// <returns>Returns the current aggregate instance which its type is converted to the role interface.</returns>
        public TRole ActAs<TRole>() where TRole : class
        {
            if (!typeof(TRole).IsInterface)
            {
                throw new Exception(string.Format("TRole '{0}' must be an interface.", typeof(TRole).FullName));
            }

            var role = this as TRole;

            if (role == null)
            {
                throw new Exception(string.Format("AggregateRoot '{0}' can not act as role '{1}'.", this.GetType().FullName, typeof(TRole).FullName));
            }

            return role;
        }

        #endregion

        #region Protected Methods

        /// <summary>Raise a domain event.
        /// <remarks>
        /// The event first will be handled by the current aggregate, and then be queued in the local queue of the aggregate.
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

        /// <summary>Get all the uncommitted events of the current aggregate.
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

        /// <summary>Handle the given event and update the aggregate status.
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
                    throw new Exception(string.Format("Event handler not found on {0} for {1}.", this.GetType().FullName, evnt.GetType().FullName));
                }

                handler(this, evnt);
            }
        }
        /// <summary>Verify whether the given event stream can be applied on the current aggregate.
        /// </summary>
        private void VerifyEvent(EventStream eventStream)
        {
            if (eventStream.AggregateRootId != UniqueId)
            {
                var errorMessage = string.Format("Cannot apply event stream to aggregate as the AggregateRootId not matched. EventStream Id:{0}, AggregateRootId:{1}; Current AggregateRootId:{2}",
                                        eventStream.Id,
                                        eventStream.AggregateRootId,
                                        UniqueId);
                throw new Exception(errorMessage);
            }

            if (eventStream.Version != Version + 1)
            {
                var errorMessage = string.Format("Cannot apply event stream to aggregate as the version not matched. EventStream Id:{0}, Version:{1}; Current AggregateRoot Version:{2}",
                                        eventStream.Id,
                                        eventStream.Version,
                                        Version);
                throw new Exception(errorMessage);
            }
        }
        /// <summary>Apply all the events of the given event stream to the aggregate.
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
        /// <summary>Queue a uncommitted event into the current local queue.
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
    /// <summary>Abstract aggregate root base class with strong type aggregate root id.
    /// </summary>
    [Serializable]
    public abstract class AggregateRoot<TAggregateRootId> : AggregateRoot
    {
        /// <summary>
        /// 
        /// </summary>
        protected AggregateRoot() { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        protected AggregateRoot(TAggregateRootId id) : base(id.ToString()) { }

        /// <summary>The strong type Id of the aggregate.
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
