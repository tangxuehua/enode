using System.Collections.Concurrent;
using ENode.Eventing;
using ENode.Infrastructure;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using UniqueValidationSample.Events;

namespace UniqueValidationSample.EventPersistenceSynchronizers {
    [Component(LifeStyle.Singleton)]
    public class UserNameUniqueValidator : IEventPersistenceSynchronizer<UserRegistered> {
        private string _connectionString = "mongodb://localhost/UniqueValidationSampleDB";
        private string _usernameCollectionName = "UserNameCollection";
        private ConcurrentDictionary<string, MongoCollection<BsonDocument>> _collectionDict = new ConcurrentDictionary<string, MongoCollection<BsonDocument>>();

        public void OnBeforePersisting(UserRegistered evnt) {
            GetMongoCollection().Insert(new BsonDocument
            {
                { "_id", evnt.UserId.ToString() },
                { "UserName", evnt.UserName },
                { "Status", 1 }
            });
        }
        public void OnAfterPersisted(UserRegistered evnt) {
            var collection = GetMongoCollection();
            var document = collection.FindOneById(new BsonString(evnt.UserId.ToString()));
            document["Status"] = 2;
            collection.Save(document);
        }

        private MongoCollection<BsonDocument> GetMongoCollection() {
            MongoCollection<BsonDocument> collection;

            if (!_collectionDict.TryGetValue(_usernameCollectionName, out collection)) {
                lock (this) {
                    var client = new MongoClient(_connectionString);
                    var db = client.GetServer().GetDatabase(new MongoUrl(_connectionString).DatabaseName);
                    collection = db.GetCollection(_usernameCollectionName);
                    collection.EnsureIndex(IndexKeys.Ascending("UserName"), IndexOptions.SetName("UserNameUniqueIndex").SetUnique(true));
                    _collectionDict.TryAdd(_usernameCollectionName, collection);
                }
            }

            return collection;
        }
    }
}
