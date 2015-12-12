using System;

namespace ENode.Domain
{
    /// <summary>Represents a provider to provide the aggregate repository.
    /// </summary>
    public interface IAggregateRepositoryProvider
    {
        /// <summary>Get the aggregateRepository for the given aggregate type.
        /// </summary>
        /// <param name="aggregateRootType"></param>
        /// <returns></returns>
        IAggregateRepositoryProxy GetRepository(Type aggregateRootType);
    }
}
