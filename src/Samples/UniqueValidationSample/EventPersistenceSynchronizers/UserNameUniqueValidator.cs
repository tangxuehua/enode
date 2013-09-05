using System.Collections.Concurrent;
using ENode.Eventing;
using ENode.Infrastructure;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using UniqueValidationSample.Events;

namespace UniqueValidationSample.EventPersistenceSynchronizers
{
    [Component(LifeStyle.Singleton)]
    public class UserNameUniqueValidator : IEventSynchronizer<UserRegistered>
    {
        private const string ConnectionString = "mongodb://localhost/UniqueValidationSampleDB";
        private const string UsernameCollectionName = "UserNameCollection";
        private readonly ConcurrentDictionary<string, MongoCollection<BsonDocument>> _collectionDict = new ConcurrentDictionary<string, MongoCollection<BsonDocument>>();

        public void OnBeforePersisting(UserRegistered evnt)
        {
            GetMongoCollection().Insert(new BsonDocument
            {
                { "_id", evnt.UserId.ToString() },
                { "UserName", evnt.UserName },
                { "Status", 1 }
            });
        }
        public void OnAfterPersisted(UserRegistered evnt)
        {
            var collection = GetMongoCollection();
            var document = collection.FindOneById(new BsonString(evnt.UserId.ToString()));
            document["Status"] = 2;
            collection.Save(document);
        }

        private MongoCollection<BsonDocument> GetMongoCollection()
        {
            MongoCollection<BsonDocument> collection;
            if (_collectionDict.TryGetValue(UsernameCollectionName, out collection)) return collection;

            lock (this)
            {
                var client = new MongoClient(ConnectionString);
                var db = client.GetServer().GetDatabase(new MongoUrl(ConnectionString).DatabaseName);
                collection = db.GetCollection(UsernameCollectionName);
                collection.EnsureIndex(IndexKeys.Ascending("UserName"), IndexOptions.SetName("UserNameUniqueIndex").SetUnique(true));
                _collectionDict.TryAdd(UsernameCollectionName, collection);
            }

            return collection;
        }
    }
}
