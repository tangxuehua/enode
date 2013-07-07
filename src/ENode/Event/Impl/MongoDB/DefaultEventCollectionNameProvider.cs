using System;
using System.Collections.Generic;

namespace ENode.Eventing.Storage.MongoDB
{
    public class DefaultEventCollectionNameProvider : IEventCollectionNameProvider
    {
        private string _collectionName;

        public DefaultEventCollectionNameProvider(string collectionName)
        {
            if (string.IsNullOrEmpty(collectionName))
            {
                throw new ArgumentNullException("collectionName");
            }
            _collectionName = collectionName;
        }

        public string GetCollectionName(string aggregateRootId, Type aggregateRootType)
        {
            return _collectionName;
        }
        public IEnumerable<string> GetAllCollectionNames()
        {
            return new string[] { _collectionName };
        }
    }
}
