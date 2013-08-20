using System;
using System.Collections.Generic;
using System.Linq;
using ENode.Domain;

namespace ENode.Eventing.Impl.SQL
{
    /// <summary>
    /// 
    /// </summary>
    public class AggregatePerEventTableNameProvider : IEventTableNameProvider
    {
        private readonly IAggregateRootTypeProvider _aggregateRootTypeProvider;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aggregateRootTypeProvider"></param>
        public AggregatePerEventTableNameProvider(IAggregateRootTypeProvider aggregateRootTypeProvider)
        {
            _aggregateRootTypeProvider = aggregateRootTypeProvider;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aggregateRootId"></param>
        /// <param name="aggregateRootType"></param>
        /// <returns></returns>
        public string GetTable(string aggregateRootId, Type aggregateRootType)
        {
            return aggregateRootType.Name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetAllTables()
        {
            return _aggregateRootTypeProvider.GetAllAggregateRootTypes().Select(x => x.Name);
        }
    }
}
