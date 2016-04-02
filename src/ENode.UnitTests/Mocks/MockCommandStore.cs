using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using ECommon.IO;
using ENode.Commanding;

namespace ENode.UnitTests
{
    public class MockCommandStore : ICommandStore
    {
        private readonly ConcurrentDictionary<string, HandledCommand> _handledCommandDict = new ConcurrentDictionary<string, HandledCommand>();
        private int _expectAddFailedCount = 0;
        private int _expectGetFailedCount = 0;
        private int _currentFailedCount = 0;
        private FailedType _failedType;

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
    }
}
