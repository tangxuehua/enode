using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ECommon.Logging;

namespace ENode.Eventing.Impl
{
    public class InMemoryEventStore : IEventStore
    {
        private const int Editing = 1;
        private const int UnEditing = 0;
        private readonly object _lockObj = new object();
        private readonly ConcurrentDictionary<string, AggregateInfo> _aggregateInfoDict;
        private readonly ILogger _logger;

        public InMemoryEventStore(ILoggerFactory loggerFactory)
        {
            _aggregateInfoDict = new ConcurrentDictionary<string, AggregateInfo>();
            _logger = loggerFactory.Create(GetType().FullName);
        }

        public IEnumerable<DomainEventStream> QueryAggregateEvents(string aggregateRootId, string aggregateRootTypeName, int minVersion, int maxVersion)
        {
            var eventStreams = new List<DomainEventStream>();

            AggregateInfo aggregateInfo;
            if (!_aggregateInfoDict.TryGetValue(aggregateRootId, out aggregateInfo))
            {
                return eventStreams;
            }

            var min = minVersion > 1 ? minVersion : 1;
            var max = maxVersion < aggregateInfo.CurrentVersion ? maxVersion : aggregateInfo.CurrentVersion;

            return aggregateInfo.EventDict.Where(x => x.Key >= min && x.Key <= max).Select(x => x.Value).ToList();
        }
        public Task<EventAppendResult> BatchAppendAsync(IEnumerable<DomainEventStream> eventStreams)
        {
            var eventStreamDict = new Dictionary<string, IList<DomainEventStream>>();
            var aggregateRootIdList = eventStreams.Select(x => x.AggregateRootId).Distinct().ToList();
            foreach (var aggregateRootId in aggregateRootIdList)
            {
                var eventStreamList = eventStreams.Where(x => x.AggregateRootId == aggregateRootId).ToList();
                if (eventStreamList.Count > 0)
                {
                    eventStreamDict.Add(aggregateRootId, eventStreamList);
                }
            }
            var eventAppendResult = new EventAppendResult();
            foreach (var entry in eventStreamDict)
            {
                BatchAppend(entry.Key, entry.Value, eventAppendResult);
            }
            return Task.FromResult(eventAppendResult);
        }
        public Task<DomainEventStream> FindAsync(string aggregateRootId, int version)
        {
            return Task.FromResult(Find(aggregateRootId, version));
        }
        public Task<DomainEventStream> FindAsync(string aggregateRootId, string commandId)
        {
            return Task.FromResult(Find(aggregateRootId, commandId));
        }
        public Task<IEnumerable<DomainEventStream>> QueryAggregateEventsAsync(string aggregateRootId, string aggregateRootTypeName, int minVersion, int maxVersion)
        {
            return Task.FromResult(QueryAggregateEvents(aggregateRootId, aggregateRootTypeName, minVersion, maxVersion));
        }

        private void BatchAppend(string aggregateRootId, IList<DomainEventStream> eventStreamList, EventAppendResult eventAppendResult)
        {
            lock (_lockObj)
            {
                var aggregateInfo = _aggregateInfoDict.GetOrAdd(aggregateRootId, x => new AggregateInfo());

                //检查提交过来的第一个事件的版本号是否是当前聚合根的当前版本号的下一个版本号
                if (eventStreamList.First().Version != aggregateInfo.CurrentVersion + 1)
                {
                    eventAppendResult.AddDuplicateEventAggregateRootId(aggregateRootId);
                    return;
                }
                //检查提交过来的事件本身是否满足版本号的递增关系
                for (var i = 0; i < eventStreamList.Count - 1; i++)
                {
                    if (eventStreamList[i + 1].Version != eventStreamList[i].Version + 1)
                    {
                        eventAppendResult.AddDuplicateEventAggregateRootId(aggregateRootId);
                        return;
                    }
                }

                //检查重复处理的命令ID
                var duplicateCommandIds = new List<string>();
                foreach (DomainEventStream eventStream in eventStreamList)
                {
                    if (aggregateInfo.CommandDict.ContainsKey(eventStream.CommandId))
                    {
                        duplicateCommandIds.Add(eventStream.CommandId);
                    }
                }
                if (duplicateCommandIds.Count > 0)
                {
                    eventAppendResult.AddDuplicateCommandIds(aggregateRootId, duplicateCommandIds);
                    return;
                }

                foreach (DomainEventStream eventStream in eventStreamList)
                {
                    aggregateInfo.EventDict[eventStream.Version] = eventStream;
                    aggregateInfo.CommandDict[eventStream.CommandId] = eventStream;
                    aggregateInfo.CurrentVersion = eventStream.Version;
                }

                eventAppendResult.AddSuccessAggregateRootId(aggregateRootId);
            }
        }
        private DomainEventStream Find(string aggregateRootId, int version)
        {
            AggregateInfo aggregateInfo;
            if (!_aggregateInfoDict.TryGetValue(aggregateRootId, out aggregateInfo))
            {
                return null;
            }

            DomainEventStream eventStream;
            return aggregateInfo.EventDict.TryGetValue(version, out eventStream) ? eventStream : null;
        }
        private DomainEventStream Find(string aggregateRootId, string commandId)
        {
            AggregateInfo aggregateInfo;
            if (!_aggregateInfoDict.TryGetValue(aggregateRootId, out aggregateInfo))
            {
                return null;
            }

            DomainEventStream eventStream;
            return aggregateInfo.CommandDict.TryGetValue(commandId, out eventStream) ? eventStream : null;
        }
        class AggregateInfo
        {
            public long CurrentVersion;
            public ConcurrentDictionary<int, DomainEventStream> EventDict = new ConcurrentDictionary<int, DomainEventStream>();
            public ConcurrentDictionary<string, DomainEventStream> CommandDict = new ConcurrentDictionary<string, DomainEventStream>();
        }
    }
}
