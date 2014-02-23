using System;
using System.Collections.Generic;
using ENode.Eventing;
using ENode.Infrastructure;

namespace ENode.Domain
{
    /// <summary>Aggregate root base class.
    /// </summary>
    /// <typeparam name="TAggregateRootId"></typeparam>
    [Serializable]
    public abstract class AggregateRoot<TAggregateRootId> : IAggregateRoot
    {
        private TAggregateRootId _id;
        private string _uniqueId;
        private long _version;
        private Queue<IDomainEvent> _uncommittedEvents;

        /// <summary>The strong type unique id of aggregate root.
        /// </summary>
        public TAggregateRootId Id
        {
            get
            {
                return _id;
            }
            protected set
            {
                _id = value;
            }
        }

        /// <summary>Default constructor.
        /// </summary>
        protected AggregateRoot(TAggregateRootId id)
        {
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }
            _id = id;
            _uniqueId = id.ToString();
            _uncommittedEvents = new Queue<IDomainEvent>();
        }

        /// <summary>Act the current aggregate to the given type of role.
        /// <remarks>
        /// Note：the current aggregate must implement the role interface, otherwise this method will throw exception.
        /// </remarks>
        /// </summary>
        /// <typeparam name="TRole">The role interface type.</typeparam>
        /// <returns>Returns the role instance which is acted by the current aggregate.</returns>
        public TRole ActAs<TRole>() where TRole : class
        {
            if (!typeof(TRole).IsInterface)
            {
                throw new ENodeException("'{0}' is not an interface type.", typeof(TRole).Name);
            }

            var role = this as TRole;

            if (role == null)
            {
                throw new ENodeException("'{0}' can not act as role '{1}'.", this.GetType().FullName, typeof(TRole).Name);
            }

            return role;
        }

        /// <summary>Raise a domain event. The domain event will be put into the local uncommitted event queue.
        /// </summary>
        /// <param name="evnt"></param>
        protected void RaiseEvent(IDomainEvent evnt)
        {
            if (_uncommittedEvents == null)
            {
                _uncommittedEvents = new Queue<IDomainEvent>();
            }
            _uncommittedEvents.Enqueue(evnt);
        }

        /// <summary>The unique id of aggregate root, only used by framework.
        /// </summary>
        string IAggregateRoot.UniqueId
        {
            get
            {
                if (_uniqueId == null && _id != null)
                {
                    _uniqueId = _id.ToString();
                }
                return _uniqueId;
            }
            set
            {
                if (_uniqueId != null)
                {
                    throw new NotSupportedException("AggregateRoot uniqueId cannot be set twice.");
                }
                if (_version > 0)
                {
                    throw new NotSupportedException("Only the empty aggregateRoot can be set the UniqueId.");
                }
                _uniqueId = value;
            }
        }
        /// <summary>The version of aggregate root, only used by framework.
        /// </summary>
        long IAggregateRoot.Version
        {
            get
            {
                return _version;
            }
        }
        /// <summary>Returns all the uncommitted domain events of the current aggregate root, only used by framework.
        /// </summary>
        /// <returns></returns>
        IEnumerable<IDomainEvent> IAggregateRoot.GetUncommittedEvents()
        {
            return _uncommittedEvents;
        }
        /// <summary>Clear all the uncommitted domain events of the current aggregate root, only used by framework.
        /// </summary>
        void IAggregateRoot.ClearUncommittedEvents()
        {
            _uncommittedEvents.Clear();
        }

        /// <summary>Increase the version of aggregate root, only used by framework.
        /// <remarks>This method must be provided as enode will call it when rebuilding the aggregate using event sourcing.
        /// </remarks>
        /// </summary>
        private void IncreaseVersion()
        {
            _version++;
        }
    }
}
