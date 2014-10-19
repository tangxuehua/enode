using System;
using System.Collections.Generic;
using ECommon.Components;
using ENode.Eventing;
using ENode.Infrastructure;

namespace ENode.Domain
{
    /// <summary>Represents the base aggregate root.
    /// </summary>
    /// <typeparam name="TAggregateRootId"></typeparam>
    [Serializable]
    public abstract class AggregateRoot<TAggregateRootId> : IAggregateRoot
    {
        private TAggregateRootId _id;
        private string _uniqueId;
        private int _version;
        private Queue<IDomainEvent> _uncommittedEvents;

        /// <summary>The id of aggregate root.
        /// </summary>
        public TAggregateRootId Id
        {
            get
            {
                return _id;
            }
            protected set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("id");
                }
                _id = value;
                _uniqueId = value.ToString();
            }
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
                throw new Exception(string.Format("'{0}' is not an interface type.", typeof(TRole).Name));
            }

            var role = this as TRole;

            if (role == null)
            {
                throw new Exception(string.Format("'{0}' can not act as role '{1}'.", this.GetType().FullName, typeof(TRole).Name));
            }

            return role;
        }

        /// <summary>Apply a domain event, the event will just be added into the local uncommitted event queue by default.
        /// </summary>
        /// <param name="evnt"></param>
        protected void ApplyEvent(IDomainEvent evnt)
        {
            _uncommittedEvents.Enqueue(evnt);
        }

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
        }
        int IAggregateRoot.Version
        {
            get
            {
                return _version;
            }
        }
        void IAggregateRoot.ResetChanges()
        {
            if (_uncommittedEvents == null)
            {
                _uncommittedEvents = new Queue<IDomainEvent>();
            }
            else
            {
                _uncommittedEvents.Clear();
            }
        }
        IEnumerable<IDomainEvent> IAggregateRoot.GetChanges()
        {
            return _uncommittedEvents;
        }
        void IAggregateRoot.IncreaseVersion()
        {
            _version++;
        }
    }
}
