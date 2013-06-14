using System;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoQuery = MongoDB.Driver.Builders.Query;

namespace ENode.Eventing
{
    public class MongoEventHandleInfoStore : IEventHandleInfoStore
    {
        #region Private Variables

        private string _connectionString;
        private string _collectionName;

        #endregion

        #region Constructors

        public MongoEventHandleInfoStore(string connectionString, string collectionName)
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

        public void AddEventHandleInfo(Guid eventId, string eventHandlerTypeName)
        {
            var document = new BsonDocument
            {
                { "_id", new BsonDocument { { "EventId", eventId.ToString() }, { "EventHandlerTypeName", eventHandlerTypeName } } }
            };
            GetMongoCollection().Insert(document);
        }
        public bool IsEventHandleInfoExist(Guid eventId, string eventHandlerTypeName)
        {
            var id = new BsonDocument { { "EventId", eventId.ToString() }, { "EventHandlerTypeName", eventHandlerTypeName } };
            return GetMongoCollection().FindOneById(id) != null;
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
