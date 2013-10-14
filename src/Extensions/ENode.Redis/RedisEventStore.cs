using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ENode.Eventing;
using ENode.Infrastructure;
using ENode.Infrastructure.Concurrent;
using ENode.Infrastructure.Serializing;
using ServiceStack.Redis;

namespace ENode.Redis
{
    /// <summary>Redis based implementation of IEventStore.
    /// </summary>
    public class RedisEventStore : IEventStore
    {
        private readonly RedisClient _redisClient;
        private readonly IBinarySerializer _binarySerializer;

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        public RedisEventStore(string host, int port)
        {
            _redisClient = new RedisClient(host, port);
            _binarySerializer = ObjectContainer.Resolve<IBinarySerializer>();
        }

        /// <summary>Append the event stream to the event store.
        /// </summary>
        public void Append(EventStream stream)
        {
            if (stream == null)
            {
                return;
            }

            var count = _redisClient.HSetNX(stream.AggregateRootId.Replace("-", string.Empty), Encoding.UTF8.GetBytes(stream.Version.ToString()), _binarySerializer.Serialize(stream));
            if (count == 0)
            {
                throw new ConcurrentException();
            }
        }
        /// <summary>Check whether an event stream is exist in the event store.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        /// <param name="aggregateRootType"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool IsEventStreamExist(string aggregateRootId, Type aggregateRootType, Guid id)
        {
            return Query(aggregateRootId, aggregateRootType, 1, int.MaxValue).Any(x => x.Id == id);
        }
        /// <summary>Query event streams from event store.
        /// </summary>
        public IEnumerable<EventStream> Query(string aggregateRootId, Type aggregateRootType, long minStreamVersion, long maxStreamVersion)
        {
            var dataArray = _redisClient.HVals(aggregateRootId);
            var streams = new List<EventStream>();
            foreach (var data in dataArray)
            {
                var stream = _binarySerializer.Deserialize(data, typeof(EventStream)) as EventStream;
                streams.Add(stream);
            }
            return streams.Where(x => x.Version >= minStreamVersion && x.Version <= maxStreamVersion);
        }
        /// <summary>Query all the event streams from the event store.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<EventStream> QueryAll()
        {
            List<EventStream> totalStreams = new List<EventStream>();
            foreach (var key in _redisClient.GetAllKeys())
            {
                if (_redisClient.GetEntryType(key) == RedisKeyType.Hash)
                {
                    var dataArray = _redisClient.HVals(key);
                    foreach (var data in dataArray)
                    {
                        var stream = _binarySerializer.Deserialize(data, typeof(EventStream)) as EventStream;
                        totalStreams.Add(stream);
                    }
                }
            }
            return totalStreams;
        }
    }
}
