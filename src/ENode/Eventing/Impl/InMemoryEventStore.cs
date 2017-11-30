using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ECommon.IO;
using ECommon.Logging;

namespace ENode.Eventing.Impl
{
    public class InMemoryEventStore : IEventStore
    {
        private const int Editing = 1;
        private const int UnEditing = 0;
        private readonly ConcurrentDictionary<string, AggregateInfo> _aggregateInfoDict;
        private readonly ILogger _logger;

        public bool SupportBatchAppendEvent { get; set; }

        public InMemoryEventStore(ILoggerFactory loggerFactory)
        {
            _aggregateInfoDict = new ConcurrentDictionary<string, AggregateInfo>();
            SupportBatchAppendEvent = true;
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
        public Task<AsyncTaskResult<EventAppendResult>> BatchAppendAsync(IEnumerable<DomainEventStream> eventStreams)
        {
            if (!SupportBatchAppendEvent)
            {
                throw new NotSupportedException("Unsupport batch append event.");
            }
            foreach (var eventStream in eventStreams)
            {
                var task = AppendAsync(eventStream);
                if (task.Result.Data != EventAppendResult.Success)
                {
                    return task;
                }
            }
            return Task.FromResult(new AsyncTaskResult<EventAppendResult>(AsyncTaskStatus.Success, EventAppendResult.Success));
        }
        public Task<AsyncTaskResult<EventAppendResult>> AppendAsync(DomainEventStream eventStream)
        {
            return Task.FromResult(new AsyncTaskResult<EventAppendResult>(AsyncTaskStatus.Success, null, Append(eventStream)));
        }
        public Task<AsyncTaskResult<DomainEventStream>> FindAsync(string aggregateRootId, int version)
        {
            return Task.FromResult(new AsyncTaskResult<DomainEventStream>(AsyncTaskStatus.Success, null, Find(aggregateRootId, version)));
        }
        public Task<AsyncTaskResult<DomainEventStream>> FindAsync(string aggregateRootId, string commandId)
        {
            return Task.FromResult(new AsyncTaskResult<DomainEventStream>(AsyncTaskStatus.Success, null, Find(aggregateRootId, commandId)));
        }
        public Task<AsyncTaskResult<IEnumerable<DomainEventStream>>> QueryAggregateEventsAsync(string aggregateRootId, string aggregateRootTypeName, int minVersion, int maxVersion)
        {
            return Task.FromResult(new AsyncTaskResult<IEnumerable<DomainEventStream>>(AsyncTaskStatus.Success, null, QueryAggregateEvents(aggregateRootId, aggregateRootTypeName, minVersion, maxVersion)));
        }

        private EventAppendResult Append(DomainEventStream eventStream)
        {
            var aggregateInfo = _aggregateInfoDict.GetOrAdd(eventStream.AggregateRootId, x => new AggregateInfo());
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
                    return EventAppendResult.Success;
                }
                return EventAppendResult.DuplicateEvent;
            }
            finally
            {
                Interlocked.Exchange(ref aggregateInfo.Status, UnEditing);
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
            public int Status;
            public long CurrentVersion;
            public ConcurrentDictionary<int, DomainEventStream> EventDict = new ConcurrentDictionary<int, DomainEventStream>();
            public ConcurrentDictionary<string, DomainEventStream> CommandDict = new ConcurrentDictionary<string, DomainEventStream>();
        }
    }
}
