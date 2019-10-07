using System.Threading.Tasks;

namespace ENode.Domain.Impl
{
    public class AggregateRepositoryProxy<TAggregateRoot> : IAggregateRepositoryProxy where TAggregateRoot : IAggregateRoot
    {
        private readonly IAggregateRepository<TAggregateRoot> _aggregateRepository;

        public AggregateRepositoryProxy(IAggregateRepository<TAggregateRoot> aggregateRepository)
        {
            _aggregateRepository = aggregateRepository;
        }

        public object GetInnerObject()
        {
            return _aggregateRepository;
        }
        public async Task<IAggregateRoot> GetAsync(string aggregateRootId)
        {
            return await _aggregateRepository.GetAsync(aggregateRootId).ConfigureAwait(false);
        }
    }
}
