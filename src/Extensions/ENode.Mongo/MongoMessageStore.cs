using System;
using System.Collections.Generic;
using System.Linq;
using ENode.Infrastructure;
using ENode.Infrastructure.Serializing;
using ENode.Messaging;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace ENode.Mongo
{
    /// <summary>MongoDB based implementation of IMessageStore.
    /// </summary>
    public class MongoMessageStore : IMessageStore
    {
        #region Private Variables

        private readonly IQueueCollectionNameProvider _queueCollectionNameProvider;
        private readonly IBinarySerializer _binarySerializer;
        private readonly string _connectionString;

        #endregion

        #region Constructors

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <exception cref="ArgumentNullException"></exception>
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

        /// <summary>Initialize the given message queue.
        /// </summary>
        /// <param name="queueName"></param>
        public void Initialize(string queueName) { }
        /// <summary>Persist a new message to the queue.
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="message"></param>
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
        /// <summary>Remove a existing message from the queue.
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="message"></param>
        public void RemoveMessage(string queueName, IMessage message)
        {
            var collectionName = _queueCollectionNameProvider.GetCollectionName(queueName);
            var collection = GetMongoCollection(collectionName);

            collection.Remove(Query.EQ("_id", message.Id.ToString()));
        }
        /// <summary>Get all the existing messages of the queue.
        /// </summary>
        /// <param name="queueName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IEnumerable<T> GetMessages<T>(string queueName) where T : class, IMessage
        {
            var collectionName = _queueCollectionNameProvider.GetCollectionName(queueName);
            var collection = GetMongoCollection(collectionName);
            var documents = collection.FindAll();

            return documents.Select(document => (T) _binarySerializer.Deserialize(document["MessageData"].AsByteArray)).ToList();
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
