using System;

namespace ENode.Domain.Impl
{
    public class DefaultAggregateSnapshotter : IAggregateSnapshotter
    {
        private readonly IAggregateRepositoryProvider _aggregateRepositoryProvider;

        public DefaultAggregateSnapshotter(IAggregateRepositoryProvider aggregateRepositoryProvider)
        {
            _aggregateRepositoryProvider = aggregateRepositoryProvider;
        }

        public IAggregateRoot RestoreFromSnapshot(Type aggregateRootType, string aggregateRootId)
        {
            var aggregateRepository = _aggregateRepositoryProvider.GetRepository(aggregateRootType);
            if (aggregateRepository != null)
            {
                return aggregateRepository.Get(aggregateRootId);
            }
            return null;
        }
    }
}
