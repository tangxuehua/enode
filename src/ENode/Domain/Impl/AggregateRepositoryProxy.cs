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
        public IAggregateRoot Get(string aggregateRootId)
        {
            return _aggregateRepository.Get(aggregateRootId);
        }
    }
}
