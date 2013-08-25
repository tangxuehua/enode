using System;
using System.Collections.Generic;

namespace ENode.Mongo
{
    /// <summary>The default implementation of IEventCollectionNameProvider.
    /// </summary>
    public class DefaultEventCollectionNameProvider : IEventCollectionNameProvider
    {
        private readonly string _collectionName;

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="collectionName"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public DefaultEventCollectionNameProvider(string collectionName)
        {
            if (string.IsNullOrEmpty(collectionName))
            {
                throw new ArgumentNullException("collectionName");
            }
            _collectionName = collectionName;
        }

        /// <summary>Get the collection name for the given aggregate.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        /// <param name="aggregateRootType"></param>
        /// <returns></returns>
        public string GetCollectionName(string aggregateRootId, Type aggregateRootType)
        {
            return _collectionName;
        }
        /// <summary>Get all the collection names of the eventstore.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetAllCollectionNames()
        {
            return new string[] { _collectionName };
        }
    }
}
