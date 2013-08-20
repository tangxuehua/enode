using System;
using System.Collections.Generic;

namespace ENode.Mongo
{
    /// <summary>
    /// 
    /// </summary>
    public class DefaultEventCollectionNameProvider : IEventCollectionNameProvider
    {
        private readonly string _collectionName;

        /// <summary>
        /// 
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aggregateRootId"></param>
        /// <param name="aggregateRootType"></param>
        /// <returns></returns>
        public string GetCollectionName(string aggregateRootId, Type aggregateRootType)
        {
            return _collectionName;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetAllCollectionNames()
        {
            return new string[] { _collectionName };
        }
    }
}
