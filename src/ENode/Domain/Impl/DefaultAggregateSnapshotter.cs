using System;
using System.Threading.Tasks;

namespace ENode.Domain.Impl
{
    public class DefaultAggregateSnapshotter : IAggregateSnapshotter
    {
        private readonly IAggregateRepositoryProvider _aggregateRepositoryProvider;

        public DefaultAggregateSnapshotter(IAggregateRepositoryProvider aggregateRepositoryProvider)
        {
            _aggregateRepositoryProvider = aggregateRepositoryProvider;
        }

        public async Task<IAggregateRoot> RestoreFromSnapshotAsync(Type aggregateRootType, string aggregateRootId)
        {
            var aggregateRepository = _aggregateRepositoryProvider.GetRepository(aggregateRootType);
            if (aggregateRepository != null)
            {
                return await aggregateRepository.GetAsync(aggregateRootId).ConfigureAwait(false);
            }
            return null;
        }
    }
}
