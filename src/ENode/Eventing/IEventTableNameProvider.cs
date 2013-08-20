using System;
using System.Collections.Generic;

namespace ENode.Eventing
{
    /// <summary>Represents a provider to provide the eventstore event table name.
    /// </summary>
    public interface IEventTableNameProvider
    {
        /// <summary>Get table for a specific aggregate root.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        /// <param name="aggregateRootType"></param>
        /// <returns></returns>
        string GetTable(string aggregateRootId, Type aggregateRootType);
        /// <summary>Get all the tables of the eventstore.
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetAllTables();
    }
}
