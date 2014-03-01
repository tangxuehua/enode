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
        private readonly ConcurrentDictionary<string, AggregateVersionInfo> aggregateCurrentVersionDict;
        private readonly ICommitLog _commitLog;
        private readonly ILogger _logger;
        private bool _started;

        public DefaultEventStore(ICommitLog commitLog, ILoggerFactory loggerFactory)
        {
            aggregateCurrentVersionDict = new ConcurrentDictionary<string, AggregateVersionInfo>();
            _commitLog = commitLog;
            _logger = loggerFactory.Create(GetType().Name);
            _started = false;
        }

        public EventCommitStatus Commit(EventStream stream)
        {
            if (!_started)
            {
                throw new ENodeException("EventStore has not been started, cannot allow committing event stream.");
            }

            var aggregateVersionInfo = aggregateCurrentVersionDict.GetOrAdd(stream.AggregateRootId, new AggregateVersionInfo());
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

        public void Start()
        {
            RecoverAggregateVersionInfoDict();
            _started = true;
        }
        public void Shutdown()
        {
        }

        public EventStream GetEventStream(string aggregateRootId, Guid commandId)
        {
            AggregateVersionInfo aggregateVersionInfo;
            if (!aggregateCurrentVersionDict.TryGetValue(aggregateRootId, out aggregateVersionInfo))
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
            AggregateVersionInfo aggregateVersionInfo;
            if (!aggregateCurrentVersionDict.TryGetValue(aggregateRootId, out aggregateVersionInfo))
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
            var baseCommitSequence = 1;
            var commitLogPageIndex = 0L;
            var commitLogSize = 1000;
            var startCommitSequence = baseCommitSequence + commitLogPageIndex * commitLogSize;
            var eventStreams = _commitLog.Query(startCommitSequence, commitLogSize);

            while (eventStreams.Count() > 0)
            {
                var index = 0;
                foreach (var stream in eventStreams)
                {
                    var aggregateVersionInfo = aggregateCurrentVersionDict.GetOrAdd(stream.AggregateRootId, new AggregateVersionInfo());
                    var commitSequence = startCommitSequence + index;
                    try
                    {
                        aggregateVersionInfo.VersionDict.Add(stream.Version, commitSequence);
                        aggregateVersionInfo.CommandDict.Add(stream.CommandId, commitSequence);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(string.Format("Invalid EventStream found when recovering EventStore, EventStream:{0}", stream), ex);
                        throw;
                    }

                    aggregateVersionInfo.CurrentVersion = stream.Version;
                    index++;
                }
                if (eventStreams.Count() == commitLogSize)
                {
                    startCommitSequence = baseCommitSequence + (commitLogPageIndex++) * commitLogSize;
                    eventStreams = _commitLog.Query(startCommitSequence, commitLogSize);
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
