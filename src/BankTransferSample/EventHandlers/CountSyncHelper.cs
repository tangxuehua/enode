using System.Threading;
using System.Threading.Tasks;
using BankTransferSample.Domain;
using ECommon.IO;
using ENode.Infrastructure;

namespace BankTransferSample.EventHandlers
{
    public class CountSyncHelper : IMessageHandler<TransferTransactionCompletedEvent>
    {
        private ManualResetEvent _waitHandle = new ManualResetEvent(false);
        private int _expectedCount;
        private int _currentCount;

        public void SetExpectedCount(int expectedCount)
        {
            _expectedCount = expectedCount;
        }

        public void WaitOne()
        {
            _waitHandle.WaitOne();
        }

        public Task<AsyncTaskResult> HandleAsync(TransferTransactionCompletedEvent message)
        {
            var currentCount = Interlocked.Increment(ref _currentCount);
            if (currentCount == _expectedCount)
            {
                _waitHandle.Set();
            }
            return Task.FromResult(AsyncTaskResult.Success);
        }
    }
}
