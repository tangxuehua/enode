using System;
using System.Collections.Generic;

namespace ENode.Mongo
{
    /// <summary>Represents a provider to provide a mongo collection name.
    /// </summary>
    public interface IEventCollectionNameProvider
    {
        /// <summary>Get the collection name for the given aggregate.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        /// <param name="aggregateRootType"></param>
        /// <returns></returns>
        string GetCollectionName(string aggregateRootId, Type aggregateRootType);
        /// <summary>Get all the collection names of the eventstore.
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetAllCollectionNames();
    }
}
