using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ECommon.Logging;
using ENode.Infrastructure;

namespace ENode.Eventing.Impl.InMemory
{
    public class InMemoryEventStore : IEventStore
    {
        private const int Editing = 1;
        private const int UnEditing = 0;
        private readonly ConcurrentDictionary<string, AggregateInfo> _aggregateInfoDict;
        private readonly ConcurrentDictionary<long, DomainEventStream> _eventDict;
        private readonly ILogger _logger;
        private long _sequence;

        public InMemoryEventStore(ILoggerFactory loggerFactory)
        {
            _aggregateInfoDict = new ConcurrentDictionary<string, AggregateInfo>();
            _eventDict = new ConcurrentDictionary<long, DomainEventStream>();
            _logger = loggerFactory.Create(GetType().FullName);
        }

        public void BatchAppend(IEnumerable<DomainEventStream> eventStreams)
        {
            throw new NotSupportedException();
        }
        public EventAppendResult Append(DomainEventStream eventStream)
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
        public DomainEventStream Find(string aggregateRootId, int version)
        {
            AggregateInfo aggregateInfo;
            if (!_aggregateInfoDict.TryGetValue(aggregateRootId, out aggregateInfo))
            {
                return null;
            }

            DomainEventStream eventStream;
            return aggregateInfo.EventDict.TryGetValue(version, out eventStream) ? eventStream : null;
        }
        public DomainEventStream Find(string aggregateRootId, string commandId)
        {
            AggregateInfo aggregateInfo;
            if (!_aggregateInfoDict.TryGetValue(aggregateRootId, out aggregateInfo))
            {
                return null;
            }

            DomainEventStream eventStream;
            return aggregateInfo.CommandDict.TryGetValue(commandId, out eventStream) ? eventStream : null;
        }
        public IEnumerable<DomainEventStream> QueryAggregateEvents(string aggregateRootId, int aggregateRootTypeCode, int minVersion, int maxVersion)
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
        public IEnumerable<DomainEventStream> QueryByPage(int pageIndex, int pageSize)
        {
            var start = pageIndex * pageSize;
            return _eventDict.Skip(start).Take(pageSize).Select(x => x.Value).ToList();
        }

        public Task<AsyncOperationResult> BatchAppendAsync(IEnumerable<DomainEventStream> eventStreams)
        {
            throw new NotImplementedException();
        }
        public Task<AsyncOperationResult<EventAppendResult>> AppendAsync(DomainEventStream eventStream)
        {
            var taskCompletionSource = new TaskCompletionSource<AsyncOperationResult<EventAppendResult>>();
            taskCompletionSource.SetResult(new AsyncOperationResult<EventAppendResult>(AsyncOperationResultStatus.Success, null, Append(eventStream)));
            return taskCompletionSource.Task;
        }
        public Task<AsyncOperationResult<DomainEventStream>> FindAsync(string aggregateRootId, int version)
        {
            var taskCompletionSource = new TaskCompletionSource<AsyncOperationResult<DomainEventStream>>();
            taskCompletionSource.SetResult(new AsyncOperationResult<DomainEventStream>(AsyncOperationResultStatus.Success, null, Find(aggregateRootId, version)));
            return taskCompletionSource.Task;
        }
        public Task<AsyncOperationResult<DomainEventStream>> FindAsync(string aggregateRootId, string commandId)
        {
            var taskCompletionSource = new TaskCompletionSource<AsyncOperationResult<DomainEventStream>>();
            taskCompletionSource.SetResult(new AsyncOperationResult<DomainEventStream>(AsyncOperationResultStatus.Success, null, Find(aggregateRootId, commandId)));
            return taskCompletionSource.Task;
        }
        public Task<AsyncOperationResult<IEnumerable<DomainEventStream>>> QueryAggregateEventsAsync(string aggregateRootId, int aggregateRootTypeCode, int minVersion, int maxVersion)
        {
            var taskCompletionSource = new TaskCompletionSource<AsyncOperationResult<IEnumerable<DomainEventStream>>>();
            taskCompletionSource.SetResult(new AsyncOperationResult<IEnumerable<DomainEventStream>>(AsyncOperationResultStatus.Success, null, QueryAggregateEvents(aggregateRootId, aggregateRootTypeCode, minVersion, maxVersion)));
            return taskCompletionSource.Task;
        }
        public Task<AsyncOperationResult<IEnumerable<DomainEventStream>>> QueryByPageAsync(int pageIndex, int pageSize)
        {
            var taskCompletionSource = new TaskCompletionSource<AsyncOperationResult<IEnumerable<DomainEventStream>>>();
            taskCompletionSource.SetResult(new AsyncOperationResult<IEnumerable<DomainEventStream>>(AsyncOperationResultStatus.Success, null, QueryByPage(pageIndex, pageSize)));
            return taskCompletionSource.Task;
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
