using System;
using System.Collections.Generic;

namespace ENode.Eventing.Storage.MongoDB
{
    /// <summary>Represents a provider to provide a mongo collection name.
    /// </summary>
    public interface IMongoCollectionNameProvider
    {
        string GetCollectionName(string aggregateRootId, Type aggregateRootType);
        /// <summary>Get all the collection names of the eventstore.
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetAllCollectionNames();
    }
}
