using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using ECommon.IO;
using ENode.Commanding;
using ENode.Eventing;

namespace ENode.Tests
{
    public class MockEventStore : IEventStore
    {
        private readonly ConcurrentDictionary<string, HandledCommand> _handledCommandDict = new ConcurrentDictionary<string, HandledCommand>();
        private int _expectAddFailedCount = 0;
        private int _expectGetFailedCount = 0;
        private int _currentFailedCount = 0;
        private FailedType _failedType;

        public bool SupportBatchAppendEvent
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void Reset()
        {
            _failedType = FailedType.None;
            _expectAddFailedCount = 0;
            _expectGetFailedCount = 0;
            _currentFailedCount = 0;
        }
        public void SetExpectGetFailedCount(FailedType failedType, int count)
        {
            _failedType = failedType;
            _expectGetFailedCount = count;
        }
        public void SetExpectAddFailedCount(FailedType failedType, int count)
        {
            _failedType = failedType;
            _expectAddFailedCount = count;
        }
        public Task<AsyncTaskResult<CommandAddResult>> AddAsync(HandledCommand handledCommand)
        {
            if (_currentFailedCount < _expectAddFailedCount)
            {
                _currentFailedCount++;

                if (_failedType == FailedType.UnKnownException)
                {
                    throw new Exception("AddCommandAsyncUnKnownException" + _currentFailedCount);
                }
                else if (_failedType == FailedType.IOException)
                {
                    throw new IOException("AddCommandAsyncIOException" + _currentFailedCount);
                }
                else if (_failedType == FailedType.TaskIOException)
                {
                    return Task.FromResult(new AsyncTaskResult<CommandAddResult>(AsyncTaskStatus.Failed, "AddCommandAsyncError" + _currentFailedCount));
                }
            }
            return Task.FromResult(new AsyncTaskResult<CommandAddResult>(AsyncTaskStatus.Success, null, Add(handledCommand)));
        }
        public Task<AsyncTaskResult<HandledCommand>> GetAsync(string commandId)
        {
            if (_currentFailedCount < _expectGetFailedCount)
            {
                _currentFailedCount++;

                if (_failedType == FailedType.UnKnownException)
                {
                    throw new Exception("GetCommandAsyncUnKnownException" + _currentFailedCount);
                }
                else if (_failedType == FailedType.IOException)
                {
                    throw new IOException("GetCommandAsyncIOException" + _currentFailedCount);
                }
                else if (_failedType == FailedType.TaskIOException)
                {
                    return Task.FromResult(new AsyncTaskResult<HandledCommand>(AsyncTaskStatus.Failed, "GetCommandAsyncError" + _currentFailedCount));
                }
            }
            return Task.FromResult(new AsyncTaskResult<HandledCommand>(AsyncTaskStatus.Success, null, Get(commandId)));
        }

        private CommandAddResult Add(HandledCommand handledCommand)
        {
            if (_handledCommandDict.TryAdd(handledCommand.CommandId, handledCommand))
            {
                return CommandAddResult.Success;
            }
            return CommandAddResult.DuplicateCommand;
        }
        private HandledCommand Get(string commandId)
        {
            HandledCommand handledCommand;
            if (_handledCommandDict.TryGetValue(commandId, out handledCommand))
            {
                return handledCommand;
            }
            return null;
        }

        public IEnumerable<DomainEventStream> QueryAggregateEvents(string aggregateRootId, string aggregateRootTypeName, int minVersion, int maxVersion)
        {
            throw new NotImplementedException();
        }

        public Task<AsyncTaskResult<EventAppendResult>> BatchAppendAsync(IEnumerable<DomainEventStream> eventStreams)
        {
            throw new NotImplementedException();
        }

        public Task<AsyncTaskResult<EventAppendResult>> AppendAsync(DomainEventStream eventStream)
        {
            throw new NotImplementedException();
        }

        public Task<AsyncTaskResult<DomainEventStream>> FindAsync(string aggregateRootId, int version)
        {
            throw new NotImplementedException();
        }

        public Task<AsyncTaskResult<DomainEventStream>> FindAsync(string aggregateRootId, string commandId)
        {
            throw new NotImplementedException();
        }

        public Task<AsyncTaskResult<IEnumerable<DomainEventStream>>> QueryAggregateEventsAsync(string aggregateRootId, string aggregateRootTypeName, int minVersion, int maxVersion)
        {
            throw new NotImplementedException();
        }
    }
}
