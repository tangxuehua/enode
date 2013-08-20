using System;

namespace ENode.Domain
{
    /// <summary>Defines a factory to create empty aggregate root.
    /// </summary>
    public interface IAggregateRootFactory
    {
        /// <summary>Create an empty aggregate root with the given type.
        /// </summary>
        AggregateRoot CreateAggregateRoot(Type aggregateRootType);
    }
}
