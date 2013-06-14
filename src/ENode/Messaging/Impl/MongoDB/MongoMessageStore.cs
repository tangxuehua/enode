using System;
using System.Collections.Generic;
using ENode.Infrastructure;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoQuery = MongoDB.Driver.Builders.Query;

namespace ENode.Messaging.Storage.MongoDB
{
    public class MongoMessageStore : IMessageStore
    {
        #region Private Variables

        private IQueueCollectionNameProvider _queueCollectionNameProvider;
        private IBinarySerializer _binarySerializer;
        private string _connectionString;

        #endregion

        #region Constructors

        public MongoMessageStore(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }

            _connectionString = connectionString;

            _queueCollectionNameProvider = ObjectContainer.Resolve<IQueueCollectionNameProvider>();
            _binarySerializer = ObjectContainer.Resolve<IBinarySerializer>();
        }

        #endregion

        public void Initialize(string queueName) { }
        public void AddMessage(string queueName, IMessage message)
        {
            var collectionName = _queueCollectionNameProvider.GetCollectionName(queueName);
            var collection = GetMongoCollection(collectionName);
            var document = new BsonDocument
            {
                { "_id", message.Id.ToString() },
                { "MessageData", _binarySerializer.Serialize(message) }
            };

            collection.Insert(document);
        }
        public void RemoveMessage(string queueName, IMessage message)
        {
            var collectionName = _queueCollectionNameProvider.GetCollectionName(queueName);
            var collection = GetMongoCollection(collectionName);

            collection.Remove(MongoQuery.EQ("_id", message.Id.ToString()));
        }
        public IEnumerable<T> GetMessages<T>(string queueName) where T : class, IMessage
        {
            var collectionName = _queueCollectionNameProvider.GetCollectionName(queueName);
            var collection = GetMongoCollection(collectionName);
            var documents = collection.FindAll();
            var messages = new List<T>();

            foreach (var document in documents)
            {
                messages.Add((T)_binarySerializer.Deserialize(document["MessageData"].AsByteArray));
            }

            return messages;
        }

        private MongoCollection<BsonDocument> GetMongoCollection(string collectionName)
        {
            var client = new MongoClient(_connectionString);
            var db = client.GetServer().GetDatabase(new MongoUrl(_connectionString).DatabaseName);
            var collection = db.GetCollection(collectionName);
            return collection;
        }
    }
}
