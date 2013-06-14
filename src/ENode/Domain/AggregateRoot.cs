using System;
using System.Collections.Generic;
using System.Linq;
using ENode.Eventing;
using ENode.Infrastructure;
using ENode.Snapshoting;

namespace ENode.Domain
{
    /// <summary>Aggregate root abstract base class.
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
        public AggregateRoot(string uniqueId) : this()
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

        #region Internal Members

        /// <summary>Get all the uncommitted events of the current aggregate.
        /// </summary>
        internal IEnumerable<IEvent> GetUncommittedEvents()
        {
            return _uncommittedEvents;
        }
        /// <summary>Replay the given event stream.
        /// </summary>
        /// <param name="stream"></param>
        internal void ReplayEvent(EventStream stream)
        {
            ReplayEvents(new EventStream[] { stream });
        }
        /// <summary>Replay the given event streams.
        /// </summary>
        internal void ReplayEvents(IEnumerable<EventStream> streams)
        {
            if (_uncommittedEvents.Count() > 0)
            {
                _uncommittedEvents.Clear();
            }

            foreach (var stream in streams)
            {
                if (stream.Version == 1)
                {
                    UniqueId = stream.AggregateRootId;
                }
                VerifyEvent(stream);
                ApplyEvent(stream);
            }
        }
        /// <summary>Initialize from the given snapshot.
        /// </summary>
        internal void InitializeFromSnapshot(Snapshot snapshot)
        {
            UniqueId = snapshot.AggregateRootId;
            Version = snapshot.StreamVersion;
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
        private void VerifyEvent(EventStream stream)
        {
            if (stream.AggregateRootId != UniqueId)
            {
                var errorMessage = string.Format("Cannot apply event stream to aggregate as the AggregateRootId not matched. EventStream Id:{0}, AggregateRootId:{1}; Current AggregateRootId:{2}",
                                        stream.Id,
                                        stream.AggregateRootId,
                                        UniqueId);
                throw new Exception(errorMessage);
            }

            if (stream.Version != Version + 1)
            {
                var errorMessage = string.Format("Cannot apply event stream to aggregate as the StreamVersion not matched. EventStream Id:{0}, Version:{1}; Current AggregateRoot Version:{2}",
                                        stream.Id,
                                        stream.Version,
                                        Version);
                throw new Exception(errorMessage);
            }
        }
        /// <summary>Apply all the events of the given event stream to the aggregate.
        /// </summary>
        /// <param name="stream"></param>
        private void ApplyEvent(EventStream stream)
        {
            foreach (var evnt in stream.Events)
            {
                HandleEvent(evnt);
            }
            Version = stream.Version;
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
    /// <summary>Aggregate root abstract base class with strong type aggregate root id.
    /// </summary>
    [Serializable]
    public abstract class AggregateRoot<TAggregateRootId> : AggregateRoot
    {
        protected AggregateRoot() : base() { }
        public AggregateRoot(TAggregateRootId id) : base(id.ToString()) { }

        /// <summary>The strong type Id of the aggregate.
        /// </summary>
        public TAggregateRootId Id
        {
            get
            {
                if (UniqueId != null)
                {
                    return TypeUtils.ConvertType<TAggregateRootId>(UniqueId);
                }
                return default(TAggregateRootId);
            }
            set
            {
                base.UniqueId = TypeUtils.ConvertType<string>(value);
            }
        }
    }
}
