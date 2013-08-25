using System;
using System.Collections.Generic;
using System.Linq;
using ENode.Domain;

namespace ENode.Mongo
{
    /// <summary>The default implementation of IEventCollectionNameProvider.
    /// </summary>
    public class AggregatePerEventCollectionNameProvider : IEventCollectionNameProvider
    {
        private readonly IAggregateRootTypeProvider _aggregateRootTypeProvider;

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="aggregateRootTypeProvider"></param>
        public AggregatePerEventCollectionNameProvider(IAggregateRootTypeProvider aggregateRootTypeProvider)
        {
            _aggregateRootTypeProvider = aggregateRootTypeProvider;
        }

        /// <summary>Get the collection name for the given aggregate.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        /// <param name="aggregateRootType"></param>
        /// <returns></returns>
        public string GetCollectionName(string aggregateRootId, Type aggregateRootType)
        {
            return aggregateRootType.Name;
        }
        /// <summary>Get all the collection names of the eventstore.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetAllCollectionNames()
        {
            return _aggregateRootTypeProvider.GetAllAggregateRootTypes().Select(x => x.Name);
        }
    }
}
