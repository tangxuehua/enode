using System;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoQuery = MongoDB.Driver.Builders.Query;

namespace ENode.Eventing
{
    public class MongoEventPublishInfoStore : IEventPublishInfoStore
    {
        #region Private Variables

        private string _connectionString;
        private string _collectionName;

        #endregion

        #region Constructors

        public MongoEventPublishInfoStore(string connectionString,string collectionName)
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

        public void InsertFirstPublishedVersion(string aggregateRootId)
        {
            var document = new BsonDocument
            {
                { "AggregateRootId", aggregateRootId },
                { "Version", 1L }
            };

            GetMongoCollection().Insert(document);
        }
        public void UpdatePublishedVersion(string aggregateRootId, long version)
        {
            var collection = GetMongoCollection();
            var document = collection.FindOne(MongoQuery.EQ("AggregateRootId", aggregateRootId));
            document["Version"] = version;
            collection.Save(document);
        }
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
