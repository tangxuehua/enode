using System;
using System.Collections.Generic;

namespace ENode.Eventing.Impl.SQL
{
    /// <summary>The default implementation of IEventTableNameProvider.
    /// </summary>
    public class DefaultEventTableNameProvider : IEventTableNameProvider
    {
        private readonly string _tableName;

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="tableName"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public DefaultEventTableNameProvider(string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentNullException("tableName");
            }
            _tableName = tableName;
        }
        /// <summary>Get table for a specific aggregate root.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        /// <param name="aggregateRootType"></param>
        /// <returns></returns>
        public string GetTable(string aggregateRootId, Type aggregateRootType)
        {
            return _tableName;
        }
        /// <summary>Get all the tables of the eventstore.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetAllTables()
        {
            return new[] { _tableName };
        }
    }
}
