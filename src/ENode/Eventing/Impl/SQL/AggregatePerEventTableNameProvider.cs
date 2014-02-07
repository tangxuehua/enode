using System;
using System.Collections.Generic;
using System.Linq;
using ENode.Domain;

namespace ENode.Eventing.Impl.SQL
{
    /// <summary>Default implementation of IEventTableNameProvider.
    /// </summary>
    public class AggregatePerEventTableNameProvider : IEventTableNameProvider
    {
        private readonly IAggregateRootTypeProvider _aggregateRootTypeProvider;

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="aggregateRootTypeProvider"></param>
        public AggregatePerEventTableNameProvider(IAggregateRootTypeProvider aggregateRootTypeProvider)
        {
            _aggregateRootTypeProvider = aggregateRootTypeProvider;
        }

        /// <summary>Get table for a specific aggregate root.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        /// <param name="aggregateRootName"></param>
        /// <returns></returns>
        public string GetTable(string aggregateRootId, string aggregateRootName)
        {
            return aggregateRootName;
        }

        /// <summary>Get all the tables of the eventstore.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetAllTables()
        {
            return _aggregateRootTypeProvider.GetAllAggregateRootTypes().Select(x => x.Name);
        }
    }
}
