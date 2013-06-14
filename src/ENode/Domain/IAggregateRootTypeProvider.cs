using System;
using System.Collections.Generic;

namespace ENode.Domain
{
    /// <summary>Represents a provider to provide the aggregate root type information.
    /// </summary>
    public interface IAggregateRootTypeProvider
    {
        /// <summary>Get the aggregate root type name by the aggregate root type.
        /// </summary>
        /// <returns></returns>
        string GetAggregateRootTypeName(Type aggregateRootType);
        /// <summary>Get the aggregate root type by the aggregate root type name.
        /// </summary>
        /// <returns></returns>
        Type GetAggregateRootType(string name);
        /// <summary>Get all the aggregate root types.
        /// </summary>
        /// <returns></returns>
        IEnumerable<Type> GetAllAggregateRootTypes();
    }
}
