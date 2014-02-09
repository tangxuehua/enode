using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ECommon.Extensions;
using ECommon.Scheduling;
using ENode.Infrastructure;

namespace ENode.Eventing.Impl.InMemory
{
    /// <summary>Default event store implementation.
    /// </summary>
    public class DefaultEventStore : IEventStore
    {
        private const char KeySeperator = '|';
        private readonly ConcurrentDictionary<Guid, long> _commandIndexDict = new ConcurrentDictionary<Guid, long>();
        private readonly ConcurrentDictionary<string, long> _versionIndexDict = new ConcurrentDictionary<string, long>();
        private readonly ConcurrentDictionary<string, long> _aggregateVersionDict = new ConcurrentDictionary<string, long>();
        private readonly IScheduleService _scheduleService;
        private readonly ICommitLog _commitLog;
        private readonly ICommandIndexStore _commandIndexStore;
        private readonly IVersionIndexStore _versionIndexStore;
        private readonly ConcurrentDictionary<long, Guid> _commandIndexTempDict = new ConcurrentDictionary<long, Guid>();
        private readonly ConcurrentDictionary<long, string> _versionIndexTempDict = new ConcurrentDictionary<long, string>();
        private long _lastSavedCommandIndexSequence;
        private long _lastSavedVersionIndexSequence;
        private IList<int> _taskIds = new List<int>();
        private bool _started;

        public DefaultEventStore(ICommitLog commitLog, ICommandIndexStore commandIndexStore, IVersionIndexStore versionIndexStore, IScheduleService scheduleService)
        {
            _commitLog = commitLog;
            _commandIndexStore = commandIndexStore;
            _versionIndexStore = versionIndexStore;
            _scheduleService = scheduleService;
            _lastSavedCommandIndexSequence = 0;
            _lastSavedVersionIndexSequence = 0;
            _started = false;
        }

        public EventCommitStatus Commit(EventStream stream)
        {
            if (!_started)
            {
                throw new ENodeException("EventStore has not been started, cannot allow committing event stream.");
            }
            var commitSequence = _commitLog.Append(stream);

            if (!_commandIndexDict.TryAdd(stream.CommandId, commitSequence))
            {
                return EventCommitStatus.DuplicateCommit;
            }
            else
            {
                if (!_commandIndexTempDict.TryAdd(commitSequence, stream.CommandId))
                {
                    throw new ENodeException("Add commandId to temp dict failed, commandId:{0},commitSequence:{1}", stream.CommandId, commitSequence);
                }
            }

            var aggregateRootId = stream.AggregateRootId.ToString();
            if (stream.Version == 1)
            {
                if (!_aggregateVersionDict.TryAdd(aggregateRootId, stream.Version))
                {
                    throw new DuplicateAggregateException("Duplicate aggregate[name={0},id={1}] creation.", stream.AggregateRootName, stream.AggregateRootId);
                }
            }
            else
            {
                if (!_aggregateVersionDict.TryUpdate(aggregateRootId, stream.Version, stream.Version - 1))
                {
                    throw new ConcurrentException();
                }
            }

            var key = string.Format("{0}{2}{1}", aggregateRootId, stream.Version, KeySeperator);
            if (_versionIndexDict.TryAdd(key, commitSequence))
            {
                if (!_versionIndexTempDict.TryAdd(commitSequence, key))
                {
                    throw new ENodeException("Add version index to temp dict failed, key:{0},commitSequence:{1}", key, commitSequence);
                }
            }
            else
            {
                throw new ENodeException("Add version index to dict failed, key:{0},commitSequence:{1}", key, commitSequence);
            }

            return EventCommitStatus.Success;
        }

        public void Start()
        {
            Recover();
            _taskIds.Add(_scheduleService.ScheduleTask(SaveCommandIndex, 5000, 5000));
            _taskIds.Add(_scheduleService.ScheduleTask(SaveVersionIndex, 5000, 5000));
            _started = true;
        }
        public void Shutdown()
        {
            foreach (var taskId in _taskIds)
            {
                _scheduleService.ShutdownTask(taskId);
            }
        }

        public EventStream GetEventStream(Guid commandId)
        {
            long commitSequence;
            if (_commandIndexDict.TryGetValue(commandId, out commitSequence))
            {
                return _commitLog.Get(commitSequence);
            }
            return null;
        }
        public IEnumerable<EventStream> Query(string aggregateRootId, string aggregateRootName, long minStreamVersion, long maxStreamVersion)
        {
            long currentVersion;
            if (_aggregateVersionDict.TryGetValue(aggregateRootId, out currentVersion))
            {
                var minVersion = minStreamVersion > 1 ? minStreamVersion : 1;
                var maxVersion = maxStreamVersion < currentVersion ? maxStreamVersion : currentVersion;

                var eventStreamList = new List<EventStream>();
                for (var version = minVersion; version <= maxVersion; version++)
                {
                    var key = string.Format("{0}{2}{1}", aggregateRootId, version, KeySeperator);
                    long commitSequence;
                    if (!_versionIndexDict.TryGetValue(key, out commitSequence))
                    {
                        throw new ENodeException("Event commit sequence cannot be found of aggregate [name={0},id={1},version:{2}].", aggregateRootName, aggregateRootId, version);
                    }
                    var eventStream = _commitLog.Get(commitSequence);
                    if (eventStream == null)
                    {
                        throw new ENodeException("Event stream cannot be found from commit log, commit sequence:{0}, aggregate [name={1},id={2},version:{3}].", commitSequence, aggregateRootName, aggregateRootId, version);
                    }
                    eventStreamList.Add(eventStream);
                }
                return eventStreamList;
            }
            return new EventStream[0];
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

        private void SaveCommandIndex()
        {
            var maxSaveCount = 1000;
            var nextSaveSequence = _lastSavedCommandIndexSequence + 1;
            var savedCount = 0;
            var savedSequence = _lastSavedCommandIndexSequence;
            Guid commandId;
            while (_commandIndexTempDict.TryGetValue(nextSaveSequence, out commandId) && savedCount < maxSaveCount)
            {
                _commandIndexStore.Append(commandId, nextSaveSequence);
                _commandIndexTempDict.Remove(nextSaveSequence);
                savedSequence = nextSaveSequence;
                nextSaveSequence++;
                savedCount++;
            }
            _lastSavedCommandIndexSequence = savedSequence;
        }
        private void SaveVersionIndex()
        {
            var maxSaveCount = 1000;
            var nextSaveSequence = _lastSavedVersionIndexSequence + 1;
            var savedCount = 0;
            var savedSequence = _lastSavedVersionIndexSequence;
            string key;
            while (_versionIndexTempDict.TryGetValue(nextSaveSequence, out key) && savedCount < maxSaveCount)
            {
                _versionIndexStore.Append(key, nextSaveSequence);
                _versionIndexTempDict.Remove(nextSaveSequence);
                savedSequence = nextSaveSequence;
                nextSaveSequence++;
                savedCount++;
            }
            _lastSavedVersionIndexSequence = savedSequence;
        }
        private void Recover()
        {
            RecoverCommandIndexDict();
            RecoverVersionIndexDict();
            RecoverAggregateVersionDict();
        }
        private void RecoverCommandIndexDict()
        {
            var pageIndex = 0L;
            var size = 1000;
            var entries = _commandIndexStore.Query(pageIndex * size, size);
            var maxCommitSequence = 0L;

            while (entries.Count() > 0)
            {
                foreach (var entry in entries)
                {
                    if (!_commandIndexDict.TryAdd(entry.Key, entry.Value))
                    {
                        throw new ENodeException("Duplicate commandId:{0}", entry.Key);
                    }
                    if (entry.Value > maxCommitSequence)
                    {
                        maxCommitSequence = entry.Value;
                    }
                }
                if (entries.Count() == size)
                {
                    entries = _commandIndexStore.Query((pageIndex++) * size, size);
                }
                else
                {
                    break;
                }
            }

            _lastSavedCommandIndexSequence = maxCommitSequence;

            var baseCommitSequence = maxCommitSequence + 1;
            var commitLogPageIndex = 0L;
            var commitLogSize = 1000;
            var startCommitSequence = baseCommitSequence + commitLogPageIndex * commitLogSize;
            var eventStreams = _commitLog.Query(startCommitSequence, commitLogSize);

            while (eventStreams.Count() > 0)
            {
                var index = 0;
                foreach (var eventStream in eventStreams)
                {
                    if (!_commandIndexDict.TryAdd(eventStream.CommandId, startCommitSequence + index))
                    {
                        throw new ENodeException("Duplicate commandId:{0}", eventStream.CommandId);
                    }
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
        private void RecoverVersionIndexDict()
        {
            var pageIndex = 0L;
            var size = 1000;
            var entries = _versionIndexStore.Query(pageIndex * size, size);
            var maxCommitSequence = 0L;

            while (entries.Count() > 0)
            {
                foreach (var entry in entries)
                {
                    if (!_versionIndexDict.TryAdd(entry.Key, entry.Value))
                    {
                        throw new ENodeException("Duplicate versionIndex key:{0}", entry.Key);
                    }
                    if (entry.Value > maxCommitSequence)
                    {
                        maxCommitSequence = entry.Value;
                    }
                }
                if (entries.Count() == size)
                {
                    entries = _versionIndexStore.Query((pageIndex++) * size, size);
                }
                else
                {
                    break;
                }
            }

            _lastSavedVersionIndexSequence = maxCommitSequence;

            var baseCommitSequence = maxCommitSequence + 1;
            var commitLogPageIndex = 0L;
            var commitLogSize = 1000;
            var startCommitSequence = baseCommitSequence + commitLogPageIndex * commitLogSize;
            var eventStreams = _commitLog.Query(startCommitSequence, commitLogSize);

            while (eventStreams.Count() > 0)
            {
                var index = 0;
                foreach (var eventStream in eventStreams)
                {
                    var key = string.Format("{0}{2}{1}", eventStream.AggregateRootId, eventStream.Version, KeySeperator);
                    if (!_versionIndexDict.TryAdd(key, startCommitSequence + index))
                    {
                        throw new ENodeException("Duplicate versionIndex key:{0}", key);
                    }
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
        private void RecoverAggregateVersionDict()
        {
            foreach (var key in _versionIndexDict.Keys)
            {
                var items = key.Split(KeySeperator);
                var aggregateRootId = items[0];
                var version = long.Parse(items[1]);
                var result = _aggregateVersionDict.GetOrAdd(aggregateRootId, version);
                if (version > result)
                {
                    _aggregateVersionDict[aggregateRootId] = version;
                }
            }
        }
    }
}
