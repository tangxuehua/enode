using System.Threading;
using BankTransferSample.DomainEvents;
using ECommon.Components;
using ENode.Eventing;
using ENode.Infrastructure;

namespace BankTransferSample.EventHandlers
{
    [Component]
    public class CountSyncHelper : IEventHandler<TransferTransactionCompletedEvent>
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

        public void Handle(IHandlingContext context, TransferTransactionCompletedEvent message)
        {
            var currentCount = Interlocked.Increment(ref _currentCount);
            if (currentCount == _expectedCount)
            {
                _waitHandle.Set();
            }
        }
    }
}
