using ENode.Infrastructure;

namespace ENode.Domain
{
    public interface IAggregateRepositoryProxy : IObjectProxy
    {
        IAggregateRoot Get(string aggregateRootId);
    }
    /// <summary>Represents an aggregate repository.
    /// </summary>
    public interface IAggregateRepository<TAggregateRoot> where TAggregateRoot : IAggregateRoot
    {
        /// <summary>Get aggregate by id.
        /// </summary>
        TAggregateRoot Get(string aggregateRootId);
    }
}
