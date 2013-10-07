using System;
using System.Collections.Generic;
using System.Linq;
using ENode.Domain;
using ENode.Eventing;
using ENode.Infrastructure;
using ENode.Infrastructure.Concurrent;
using ENode.Infrastructure.Serializing;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoQuery = MongoDB.Driver.Builders.Query;

namespace ENode.Mongo
{
    /// <summary>MongoDB based implementation of IEventStore.
    /// </summary>
    public class MongoEventStore : IEventStore
    {
        private readonly IEventCollectionNameProvider _eventCollectionNameProvider;
        private readonly IBinarySerializer _binarySerializer;
        private readonly IAggregateRootTypeProvider _aggregateRootTypeProvider;
        private readonly string _connectionString;

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public MongoEventStore(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }

            _connectionString = connectionString;

            _eventCollectionNameProvider = ObjectContainer.Resolve<IEventCollectionNameProvider>();
            _binarySerializer = ObjectContainer.Resolve<IBinarySerializer>();
            _aggregateRootTypeProvider = ObjectContainer.Resolve<IAggregateRootTypeProvider>();
        }
        /// <summary>Append the event stream to the event store.
        /// </summary>
        /// <param name="stream"></param>
        public void Append(EventStream stream)
        {
            if (stream == null)
            {
                return;
            }

            try
            {
                var aggregateRootType = _aggregateRootTypeProvider.GetAggregateRootType(stream.AggregateRootName);
                var collectionName = _eventCollectionNameProvider.GetCollectionName(stream.AggregateRootId, aggregateRootType);
                var collection = GetMongoCollection(collectionName);
                collection.Insert(ToMongoEventStream(stream));
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("duplicate key error index"))
                {
                    throw new ConcurrentException();
                }
                throw;
            }
        }
        /// <summary>Query event streams from event store.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        /// <param name="aggregateRootType"></param>
        /// <param name="minStreamVersion"></param>
        /// <param name="maxStreamVersion"></param>
        /// <returns></returns>
        public IEnumerable<EventStream> Query(string aggregateRootId, Type aggregateRootType, long minStreamVersion, long maxStreamVersion)
        {
            var collectionName = _eventCollectionNameProvider.GetCollectionName(aggregateRootId, aggregateRootType);
            var collection = GetMongoCollection(collectionName);

            var query = MongoQuery.And(
                MongoQuery.EQ("AggregateRootId", aggregateRootId),
                MongoQuery.GTE("Version", minStreamVersion),
                MongoQuery.LTE("Version", maxStreamVersion));

            var documents = collection.Find(query).SetSortOrder("Version");
            var events = documents.Select(ToEventStream);

            return events;
        }
        /// <summary>Check whether an event stream is exist in the event store.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        /// <param name="aggregateRootType"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool IsEventStreamExist(string aggregateRootId, Type aggregateRootType, Guid id)
        {
            var collectionName = _eventCollectionNameProvider.GetCollectionName(aggregateRootId, aggregateRootType);
            var collection = GetMongoCollection(collectionName);
            var query = MongoQuery.EQ("Id", id.ToString());
            var count = collection.Count(query);

            return count > 0;
        }
        /// <summary>Query all the event streams from the event store.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<EventStream> QueryAll()
        {
            var collectionNames = _eventCollectionNameProvider.GetAllCollectionNames();
            var streams = new List<EventStream>();

            foreach (var documents in collectionNames.Select(GetMongoCollection).Select(collection => collection.FindAll().SetSortOrder("AggregateRootId", "Version")))
            {
                streams.AddRange(documents.Select(ToEventStream));
            }

            return streams;
        }

        private MongoCollection<BsonDocument> GetMongoCollection(string collectionName)
        {
            var client = new MongoClient(_connectionString);
            var db = client.GetServer().GetDatabase(new MongoUrl(_connectionString).DatabaseName);
            var collection = db.GetCollection(collectionName);
            return collection;
        }
        private BsonDocument ToMongoEventStream(EventStream stream)
        {
            var events = stream.Events.Select(x => _binarySerializer.Serialize(x));
            var document = new BsonDocument
            {
                { "_id", new BsonDocument { { "AggregateRootId", stream.AggregateRootId }, { "Version", stream.Version } } },
                { "Id", stream.Id.ToString() },
                { "AggregateRootName", stream.AggregateRootName },
                { "AggregateRootId", stream.AggregateRootId },
                { "Version", stream.Version },
                { "CommandId", stream.CommandId.ToString() },
                { "Timestamp", stream.Timestamp },
                { "Events", new BsonArray(events) }
            };
            return document;
        }
        private EventStream ToEventStream(BsonDocument doc)
        {
            var id = new Guid(doc["Id"].AsString);
            var aggregateRootName = doc["AggregateRootName"].AsString;
            var aggregateRootId = doc["AggregateRootId"].AsString;
            var version = doc["Version"].AsInt64;
            var timestamp = doc["Timestamp"].ToUniversalTime();
            var commandId = new Guid(doc["CommandId"].AsString);
            //TODO
            //var events = doc["Events"].AsBsonArray.Select(x => (IEvent)_binarySerializer.Deserialize(x.AsByteArray));

            //return new EventStream(id, aggregateRootId, aggregateRootName, version, commandId, timestamp, events);
            return null;
        }
    }
}
