using System;
using System.Collections.Generic;
using System.Linq;
using ENode.Domain;

namespace ENode.Mongo
{
    /// <summary>
    /// 
    /// </summary>
    public class AggregatePerEventCollectionNameProvider : IEventCollectionNameProvider
    {
        private readonly IAggregateRootTypeProvider _aggregateRootTypeProvider;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aggregateRootTypeProvider"></param>
        public AggregatePerEventCollectionNameProvider(IAggregateRootTypeProvider aggregateRootTypeProvider)
        {
            _aggregateRootTypeProvider = aggregateRootTypeProvider;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aggregateRootId"></param>
        /// <param name="aggregateRootType"></param>
        /// <returns></returns>
        public string GetCollectionName(string aggregateRootId, Type aggregateRootType)
        {
            return aggregateRootType.Name;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetAllCollectionNames()
        {
            return _aggregateRootTypeProvider.GetAllAggregateRootTypes().Select(x => x.Name);
        }
    }
}
