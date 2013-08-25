using System;
using ENode.Eventing;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoQuery = MongoDB.Driver.Builders.Query;

namespace ENode.Mongo
{
    /// <summary>MongoDB based implementation of IEventPublishInfoStore.
    /// </summary>
    public class MongoEventPublishInfoStore : IEventPublishInfoStore
    {
        #region Private Variables

        private readonly string _connectionString;
        private readonly string _collectionName;

        #endregion

        #region Constructors

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="collectionName"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public MongoEventPublishInfoStore(string connectionString, string collectionName)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }
            if (string.IsNullOrEmpty(collectionName))
            {
                throw new ArgumentNullException("collectionName");
            }

            _connectionString = connectionString;
            _collectionName = collectionName;
        }

        #endregion

        /// <summary>Insert the first published event version of aggregate.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        public void InsertFirstPublishedVersion(string aggregateRootId)
        {
            var document = new BsonDocument
            {
                { "AggregateRootId", aggregateRootId },
                { "Version", 1L }
            };

            GetMongoCollection().Insert(document);
        }
        /// <summary>Update the published event version of aggregate.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        /// <param name="version"></param>
        public void UpdatePublishedVersion(string aggregateRootId, long version)
        {
            var collection = GetMongoCollection();
            var document = collection.FindOne(MongoQuery.EQ("AggregateRootId", aggregateRootId));
            document["Version"] = version;
            collection.Save(document);
        }
        /// <summary>Get the current event published version for the specified aggregate.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        /// <returns></returns>
        public long GetEventPublishedVersion(string aggregateRootId)
        {
            var document = GetMongoCollection().FindOne(MongoQuery.EQ("AggregateRootId", aggregateRootId));
            return document["Version"].AsInt64;
        }

        private MongoCollection<BsonDocument> GetMongoCollection()
        {
            var client = new MongoClient(_connectionString);
            var db = client.GetServer().GetDatabase(new MongoUrl(_connectionString).DatabaseName);
            var collection = db.GetCollection(_collectionName);
            return collection;
        }
    }
}
