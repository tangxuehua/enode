using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ECommon.Components;
using ECommon.IO;
using ECommon.Logging;
using ENode.Eventing;
using ENode.Eventing.Impl;

namespace ENode.Tests
{
    public class MockEventStore : IEventStore
    {
        private int _expectFailedCount = 0;
        private int _currentFailedCount = 0;
        private FailedType _failedType;
        private InMemoryEventStore _inMemoryEventStore = new InMemoryEventStore(ObjectContainer.Resolve<ILoggerFactory>());

        public bool SupportBatchAppendEvent { get; set; }

        public MockEventStore()
        {
            SupportBatchAppendEvent = true;
        }

        public void Reset()
        {
            _failedType = FailedType.None;
            _expectFailedCount = 0;
            _currentFailedCount = 0;
        }
        public void SetExpectFailedCount(FailedType failedType, int count)
        {
            _failedType = failedType;
            _expectFailedCount = count;
        }

        public Task<AsyncTaskResult<EventAppendResult>> BatchAppendAsync(IEnumerable<DomainEventStream> eventStreams)
        {
            if (_currentFailedCount < _expectFailedCount)
            {
                _currentFailedCount++;

                if (_failedType == FailedType.UnKnownException)
                {
                    throw new Exception("BatchAppendAsyncUnKnownException" + _currentFailedCount);
                }
                else if (_failedType == FailedType.IOException)
                {
                    throw new IOException("BatchAppendAsyncIOException" + _currentFailedCount);
                }
                else if (_failedType == FailedType.TaskIOException)
                {
                    return Task.FromResult(new AsyncTaskResult<EventAppendResult>(AsyncTaskStatus.Failed, "BatchAppendAsyncError" + _currentFailedCount));
                }
            }
            return _inMemoryEventStore.BatchAppendAsync(eventStreams);
        }
        public Task<AsyncTaskResult<DomainEventStream>> FindAsync(string aggregateRootId, int version)
        {
            if (_currentFailedCount < _expectFailedCount)
            {
                _currentFailedCount++;

                if (_failedType == FailedType.UnKnownException)
                {
                    throw new Exception("AppendAsyncUnKnownException" + _currentFailedCount);
                }
                else if (_failedType == FailedType.IOException)
                {
                    throw new IOException("AppendAsyncIOException" + _currentFailedCount);
                }
                else if (_failedType == FailedType.TaskIOException)
                {
                    return Task.FromResult(new AsyncTaskResult<DomainEventStream>(AsyncTaskStatus.Failed, "AppendAsyncError" + _currentFailedCount));
                }
            }
            return _inMemoryEventStore.FindAsync(aggregateRootId, version);
        }
        public Task<AsyncTaskResult<DomainEventStream>> FindAsync(string aggregateRootId, string commandId)
        {
            return _inMemoryEventStore.FindAsync(aggregateRootId, commandId);
        }

        public IEnumerable<DomainEventStream> QueryAggregateEvents(string aggregateRootId, string aggregateRootTypeName, int minVersion, int maxVersion)
        {
            throw new NotImplementedException();
        }
        public Task<AsyncTaskResult<IEnumerable<DomainEventStream>>> QueryAggregateEventsAsync(string aggregateRootId, string aggregateRootTypeName, int minVersion, int maxVersion)
        {
            throw new NotImplementedException();
        }
    }
}
