using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ECommon.Logging;

namespace ENode.Eventing.Impl.InMemory
{
    /// <summary>In-memory based event store implementation. It is only used for unit test.
    /// </summary>
    public class InMemoryEventStore : IEventStore
    {
        private const int Editing = 1;
        private const int UnEditing = 0;
        private readonly ConcurrentDictionary<string, AggregateInfo> _aggregateInfoDict;
        private readonly ConcurrentDictionary<long, EventStream> _eventDict;
        private readonly ILogger _logger;
        private long _sequence;

        public InMemoryEventStore(ILoggerFactory loggerFactory)
        {
            _aggregateInfoDict = new ConcurrentDictionary<string, AggregateInfo>();
            _eventDict = new ConcurrentDictionary<long, EventStream>();
            _logger = loggerFactory.Create(GetType().FullName);
        }

        public EventAppendResult Append(EventStream eventStream)
        {
            var aggregateInfo = _aggregateInfoDict.GetOrAdd(eventStream.AggregateRootId, new AggregateInfo());
            var originalStatus = Interlocked.CompareExchange(ref aggregateInfo.Status, Editing, UnEditing);

            if (originalStatus == aggregateInfo.Status)
            {
                return EventAppendResult.DuplicateEvent;
            }

            try
            {
                if (eventStream.Version == aggregateInfo.CurrentVersion + 1)
                {
                    aggregateInfo.EventDict[eventStream.Version] = eventStream;
                    aggregateInfo.CommandDict[eventStream.CommandId] = eventStream;
                    aggregateInfo.CurrentVersion = eventStream.Version;
                    _eventDict.TryAdd(Interlocked.Increment(ref _sequence), eventStream);
                    return EventAppendResult.Success;
                }
                return EventAppendResult.DuplicateEvent;
            }
            finally
            {
                Interlocked.Exchange(ref aggregateInfo.Status, UnEditing);
            }
        }
        public EventStream Find(string aggregateRootId, int version)
        {
            AggregateInfo aggregateInfo;
            if (!_aggregateInfoDict.TryGetValue(aggregateRootId, out aggregateInfo))
            {
                return null;
            }

            EventStream eventStream;
            return aggregateInfo.EventDict.TryGetValue(version, out eventStream) ? eventStream : null;
        }
        public EventStream Find(string aggregateRootId, string commandId)
        {
            AggregateInfo aggregateInfo;
            if (!_aggregateInfoDict.TryGetValue(aggregateRootId, out aggregateInfo))
            {
                return null;
            }

            EventStream eventStream;
            return aggregateInfo.CommandDict.TryGetValue(commandId, out eventStream) ? eventStream : null;
        }
        public IEnumerable<EventStream> QueryAggregateEvents(string aggregateRootId, int aggregateRootTypeCode, int minVersion, int maxVersion)
        {
            var eventStreams = new List<EventStream>();

            AggregateInfo aggregateInfo;
            if (!_aggregateInfoDict.TryGetValue(aggregateRootId, out aggregateInfo))
            {
                return eventStreams;
            }

            var min = minVersion > 1 ? minVersion : 1;
            var max = maxVersion < aggregateInfo.CurrentVersion ? maxVersion : aggregateInfo.CurrentVersion;

            return aggregateInfo.EventDict.Where(x => x.Key >= min && x.Key <= max).Select(x => x.Value).ToList();
        }
        public IEnumerable<EventStream> QueryByPage(int pageIndex, int pageSize)
        {
            var start = pageIndex * pageSize;
            return _eventDict.Skip(start).Take(pageSize).Select(x => x.Value).ToList();
        }

        class AggregateInfo
        {
            public int Status;
            public long CurrentVersion;
            public ConcurrentDictionary<int, EventStream> EventDict = new ConcurrentDictionary<int, EventStream>();
            public ConcurrentDictionary<string, EventStream> CommandDict = new ConcurrentDictionary<string, EventStream>();
        }
    }
}
