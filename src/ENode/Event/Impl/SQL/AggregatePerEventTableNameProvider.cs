using System;
using System.Collections.Generic;
using System.Linq;
using ENode.Domain;

namespace ENode.Eventing {
    public class AggregatePerEventTableNameProvider : IEventTableNameProvider {
        private IAggregateRootTypeProvider _aggregateRootTypeProvider;

        public AggregatePerEventTableNameProvider(IAggregateRootTypeProvider aggregateRootTypeProvider) {
            _aggregateRootTypeProvider = aggregateRootTypeProvider;
        }

        public string GetTable(string aggregateRootId, Type aggregateRootType) {
            return aggregateRootType.Name;
        }
        public IEnumerable<string> GetAllTables() {
            return _aggregateRootTypeProvider.GetAllAggregateRootTypes().Select(x => x.Name);
        }
    }
}
