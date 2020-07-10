using System;
using System.Threading.Tasks;
using ECommon.IO;

namespace ENode.Domain.Impl
{
    public class DefaultAggregateSnapshotter : IAggregateSnapshotter
    {
        private readonly IAggregateRepositoryProvider _aggregateRepositoryProvider;
        private readonly IOHelper _ioHelper;

        public DefaultAggregateSnapshotter(IAggregateRepositoryProvider aggregateRepositoryProvider, IOHelper ioHelper)
        {
            _aggregateRepositoryProvider = aggregateRepositoryProvider;
            _ioHelper = ioHelper;
        }

        public async Task<IAggregateRoot> RestoreFromSnapshotAsync(Type aggregateRootType, string aggregateRootId)
        {
            var aggregateRepository = _aggregateRepositoryProvider.GetRepository(aggregateRootType);
            if (aggregateRepository != null)
            {
                return await TryGetAggregateAsync(aggregateRepository, aggregateRootType, aggregateRootId, 0, new TaskCompletionSource<IAggregateRoot>()).ConfigureAwait(false);
            }
            return null;
        }

        private Task<IAggregateRoot> TryGetAggregateAsync(IAggregateRepositoryProxy aggregateRepository, Type aggregateRootType, string aggregateRootId, int retryTimes, TaskCompletionSource<IAggregateRoot> taskSource)
        {
            _ioHelper.TryAsyncActionRecursively("TryGetAggregateAsync",
            () => aggregateRepository.GetAsync(aggregateRootId),
            currentRetryTimes => TryGetAggregateAsync(aggregateRepository, aggregateRootType, aggregateRootId, currentRetryTimes, taskSource),
            result =>
            {
                taskSource.SetResult(result);
            },
            () => string.Format("aggregateRepository.GetAsync has unknown exception, aggregateRepository: {0}, aggregateRootTypeName: {1}, aggregateRootId: {2}", aggregateRepository.GetType().FullName, aggregateRootType.FullName, aggregateRootId),
            null,
            retryTimes, true);
            return taskSource.Task;
        }
    }
}
