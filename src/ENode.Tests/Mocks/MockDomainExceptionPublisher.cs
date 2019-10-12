using System;
using System.Threading.Tasks;
using ECommon.IO;
using ENode.Domain;
using ENode.Messaging;

namespace ENode.Tests
{
    public class MockDomainExceptionPublisher : IMessagePublisher<IDomainException>
    {
        private int _expectFailedCount = 0;
        private int _currentFailedCount = 0;
        private FailedType _failedType;

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
        public Task PublishAsync(IDomainException message)
        {
            if (_currentFailedCount < _expectFailedCount)
            {
                _currentFailedCount++;

                if (_failedType == FailedType.UnKnownException)
                {
                    throw new Exception("PublishDomainExceptionAsyncUnKnownException" + _currentFailedCount);
                }
                else if (_failedType == FailedType.IOException)
                {
                    throw new IOException("PublishDomainExceptionAsyncIOException" + _currentFailedCount);
                }
            }
            return Task.CompletedTask;
        }
    }
}
