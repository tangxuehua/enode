using System;
using System.Collections.Generic;
using ENode.Eventing;

namespace ENode.Domain
{
    /// <summary>Represents an abstract base aggregate root.
    /// </summary>
    /// <typeparam name="TAggregateRootId"></typeparam>
    [Serializable]
    public abstract class AggregateRoot<TAggregateRootId> : IAggregateRoot
    {
        private Queue<IDomainEvent> _uncommittedEvents;
        protected TAggregateRootId _id;

        /// <summary>Represents the unique identifier of the aggregate root.
        /// </summary>
        public TAggregateRootId Id
        {
            get { return _id; }
        }
        /// <summary>Represents the version of the aggregate root.
        /// </summary>
        public int Version { get; private set; }

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
                throw new Exception(string.Format("'{0}' can not act as role '{1}'.", this.GetType().FullName, typeof(TRole).Name));
            }

            return actor;
        }
        /// <summary>Apply a domain event, the event will be appended to the local uncommitted event queue of the current aggregate root.
        /// </summary>
        /// <param name="domainEvent"></param>
        protected void ApplyEvent(IDomainEvent domainEvent)
        {
            _uncommittedEvents.Enqueue(domainEvent);
        }

        string IAggregateRoot.UniqueId
        {
            get
            {
                if (this.Id != null)
                {
                    return this.Id.ToString();
                }
                return null;
            }
        }
        void IAggregateRoot.ClearUncommittedEvents()
        {
            if (this._uncommittedEvents == null)
            {
                this._uncommittedEvents = new Queue<IDomainEvent>();
            }
            else
            {
                this._uncommittedEvents.Clear();
            }
        }
        IEnumerable<IDomainEvent> IAggregateRoot.GetUncommittedEvents()
        {
            return this._uncommittedEvents;
        }
        void IAggregateRoot.IncreaseVersion()
        {
            this.Version++;
        }
    }
}
