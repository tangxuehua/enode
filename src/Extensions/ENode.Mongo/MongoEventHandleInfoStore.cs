using System;
using ENode.Eventing;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ENode.Mongo
{
    /// <summary>
    /// 
    /// </summary>
    public class MongoEventHandleInfoStore : IEventHandleInfoStore
    {
        #region Private Variables

        private readonly string _connectionString;
        private readonly string _collectionName;

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="collectionName"></param>
        /// <exception cref="ArgumentNullException"></exception>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="eventHandlerTypeName"></param>
        public void AddEventHandleInfo(Guid eventId, string eventHandlerTypeName)
        {
            var document = new BsonDocument
            {
                { "_id", new BsonDocument { { "EventId", eventId.ToString() }, { "EventHandlerTypeName", eventHandlerTypeName } } }
            };
            GetMongoCollection().Insert(document);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="eventHandlerTypeName"></param>
        /// <returns></returns>
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
