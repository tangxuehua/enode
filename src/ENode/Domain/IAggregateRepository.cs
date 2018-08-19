using ENode.Infrastructure;
using System.Threading.Tasks;

namespace ENode.Domain
{
    public interface IAggregateRepositoryProxy : IObjectProxy
    {
        Task<IAggregateRoot> GetAsync(string aggregateRootId);
    }
    /// <summary>Represents an aggregate repository.
    /// </summary>
    public interface IAggregateRepository<TAggregateRoot> where TAggregateRoot : IAggregateRoot
    {
        /// <summary>Get aggregate by id.
        /// </summary>
        Task<TAggregateRoot> GetAsync(string aggregateRootId);
    }
}
