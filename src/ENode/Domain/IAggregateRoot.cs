using System.Collections.Generic;
using ENode.Eventing;

namespace ENode.Domain
{
    /// <summary>Represents an aggregate root interface.
    /// </summary>
    public interface IAggregateRoot
    {
        /// <summary>Represents the unique id of the aggregate root.
        /// </summary>
        string UniqueId { get; }
        /// <summary>Represents the current version of the aggregate root.
        /// </summary>
        int Version { get; }
        /// <summary>Returns all the uncommitted events of the aggregate root.
        /// </summary>
        /// <returns></returns>
        IEnumerable<IDomainEvent> GetUncommittedEvents();
        /// <summary>Accept the current changes, clear the uncommitted events and increase the version.
        /// </summary>
        void AcceptChanges();
        /// <summary>Increase the version of the aggregate root, this method is called when doing event sourcing.
        /// </summary>
        void IncreaseVersion();
    }
}
