using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ECommon.Logging;
using ENode.Infrastructure;

namespace ENode.Eventing.Impl.InMemory
{
    /// <summary>Default event store implementation.
    /// </summary>
    public class DefaultEventStore : IEventStore
    {
        private const int Editing = 1;
        private const int UnEditing = 0;
        private readonly ConcurrentDictionary<string, AggregateVersionInfo> _aggregateCurrentVersionDict;
        private readonly ICommitLog _commitLog;
        private readonly ILogger _logger;
        private bool _isAvailable;

        public bool IsAvailable
        {
            get { return _isAvailable; }
        }

        public DefaultEventStore(ICommitLog commitLog, ILoggerFactory loggerFactory)
        {
            _aggregateCurrentVersionDict = new ConcurrentDictionary<string, AggregateVersionInfo>();
            _commitLog = commitLog;
            _logger = loggerFactory.Create(GetType().Name);
            _isAvailable = false;
        }

        public EventCommitStatus Commit(EventStream stream)
        {
            if (!_isAvailable)
            {
                throw new ENodeException("EventStore is not available.");
            }

            var aggregateVersionInfo = _aggregateCurrentVersionDict.GetOrAdd(stream.AggregateRootId, new AggregateVersionInfo());
            var originalStatus = Interlocked.CompareExchange(ref aggregateVersionInfo.Status, Editing, UnEditing);

            if (originalStatus == aggregateVersionInfo.Status)
            {
                throw new ConcurrentException();
            }

            try
            {
                if (aggregateVersionInfo.CommandDict.ContainsKey(stream.CommandId))
                {
                    return EventCommitStatus.DuplicateCommit;
                }
                if (stream.Version == aggregateVersionInfo.CurrentVersion + 1)
                {
                    var commitSequence = _commitLog.Append(stream);
                    aggregateVersionInfo.VersionDict.Add(stream.Version, commitSequence);
                    aggregateVersionInfo.CommandDict.Add(stream.CommandId, commitSequence);
                    aggregateVersionInfo.CurrentVersion = stream.Version;
                    return EventCommitStatus.Success;
                }
                else if (stream.Version == 1)
                {
                    throw new DuplicateAggregateException("Duplicate aggregate[name={0},id={1}] creation.", stream.AggregateRootName, stream.AggregateRootId);
                }
                else
                {
                    throw new ConcurrentException();
                }
            }
            finally
            {
                Interlocked.Exchange(ref aggregateVersionInfo.Status, UnEditing);
            }
        }

        public void Initialize()
        {
            RecoverAggregateVersionInfoDict();
            _isAvailable = true;
        }

        public EventStream GetEventStream(string aggregateRootId, Guid commandId)
        {
            if (!_isAvailable)
            {
                throw new ENodeException("EventStore is not available.");
            }
            AggregateVersionInfo aggregateVersionInfo;
            if (!_aggregateCurrentVersionDict.TryGetValue(aggregateRootId, out aggregateVersionInfo))
            {
                return null;
            }

            long commitSequence;
            if (aggregateVersionInfo.CommandDict.TryGetValue(commandId, out commitSequence))
            {
                return _commitLog.Get(commitSequence);
            }
            return null;
        }
        public IEnumerable<EventStream> Query(string aggregateRootId, string aggregateRootName, long minStreamVersion, long maxStreamVersion)
        {
            if (!_isAvailable)
            {
                throw new ENodeException("EventStore is not available.");
            }
            AggregateVersionInfo aggregateVersionInfo;
            if (!_aggregateCurrentVersionDict.TryGetValue(aggregateRootId, out aggregateVersionInfo))
            {
                return new EventStream[0];
            }

            var minVersion = minStreamVersion > 1 ? minStreamVersion : 1;
            var maxVersion = maxStreamVersion < aggregateVersionInfo.CurrentVersion ? maxStreamVersion : aggregateVersionInfo.CurrentVersion;
            var eventStreamList = new List<EventStream>();
            for (var version = minVersion; version <= maxVersion; version++)
            {
                var commitSequence = aggregateVersionInfo.VersionDict[version];
                var eventStream = _commitLog.Get(commitSequence);
                if (eventStream == null)
                {
                    throw new ENodeException("Event stream cannot be found from commit log, commit sequence:{0}, aggregate [name={1},id={2},version={3}].", commitSequence, aggregateRootName, aggregateRootId, version);
                }
                eventStreamList.Add(eventStream);
            }
            return eventStreamList;
        }
        public IEnumerable<EventStream> QueryAll()
        {
            var totalStreams = new List<EventStream>();
            //TODO
            //foreach (var streams in _aggregateEventsDict.Values)
            //{
            //    totalStreams.AddRange(streams.Select(x => x.Value).ToArray());
            //}
            return totalStreams;
        }

        private void RecoverAggregateVersionInfoDict()
        {
            var firstRowNumber = 1;
            var pageIndex = 0L;
            var pageSize = 1000;
            var start = firstRowNumber + pageIndex * pageSize;
            var commitRecords = _commitLog.Query(start, pageSize);

            while (commitRecords.Count() > 0)
            {
                foreach (var commitRecord in commitRecords)
                {
                    var aggregateVersionInfo = _aggregateCurrentVersionDict.GetOrAdd(commitRecord.AggregateRootId, new AggregateVersionInfo());
                    try
                    {
                        aggregateVersionInfo.VersionDict.Add(commitRecord.Version, commitRecord.CommitSequence);
                        aggregateVersionInfo.CommandDict.Add(commitRecord.CommandId, commitRecord.CommitSequence);
                    }
                    catch (Exception ex)
                    {
                        var errorMessage = string.Format("Invalid commit record found when recovering EventStore, commitRecord:{0}", commitRecord);
                        _logger.Error(errorMessage, ex);
                        throw new ENodeException(errorMessage, ex);
                    }

                    aggregateVersionInfo.CurrentVersion = commitRecord.Version;
                }
                if (commitRecords.Count() == pageSize)
                {
                    start = firstRowNumber + (pageIndex++) * pageSize;
                    commitRecords = _commitLog.Query(start, pageSize);
                }
                else
                {
                    break;
                }
            }
        }
    }

    class AggregateVersionInfo
    {
        public int Status;
        public long CurrentVersion;
        public IDictionary<Guid, long> CommandDict = new Dictionary<Guid, long>();
        public IDictionary<long, long> VersionDict = new Dictionary<long, long>();
    }
}
