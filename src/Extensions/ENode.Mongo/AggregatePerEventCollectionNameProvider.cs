using System;
using System.Collections.Generic;
using System.Linq;
using ENode.Domain;

namespace ENode.Mongo {
    public class AggregatePerEventCollectionNameProvider : IEventCollectionNameProvider {
        private IAggregateRootTypeProvider _aggregateRootTypeProvider;

        public AggregatePerEventCollectionNameProvider(IAggregateRootTypeProvider aggregateRootTypeProvider) {
            _aggregateRootTypeProvider = aggregateRootTypeProvider;
        }

        public string GetCollectionName(string aggregateRootId, Type aggregateRootType) {
            return aggregateRootType.Name;
        }
        public IEnumerable<string> GetAllCollectionNames() {
            return _aggregateRootTypeProvider.GetAllAggregateRootTypes().Select(x => x.Name);
        }
    }
}
