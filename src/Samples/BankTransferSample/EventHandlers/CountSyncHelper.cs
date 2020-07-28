using System.Threading;
using System.Threading.Tasks;
using BankTransferSample.Domain;
using ECommon.Logging;
using ENode.Messaging;

namespace BankTransferSample.EventHandlers
{
    public class CountSyncHelper : IMessageHandler<TransferTransactionCompletedEvent>
    {
        private ManualResetEvent _waitHandle = new ManualResetEvent(false);
        private int _expectedCount;
        private int _currentCount;
        private ILogger _logger;

        public CountSyncHelper(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.Create(typeof(CountSyncHelper).Name);
        }

        public void SetExpectedCount(int expectedCount)
        {
            _expectedCount = expectedCount;
        }

        public void WaitOne()
        {
            _waitHandle.WaitOne();
        }

        public Task HandleAsync(TransferTransactionCompletedEvent message)
        {
            var currentCount = Interlocked.Increment(ref _currentCount);
            if (currentCount % 100 == 0)
            {
                _logger.InfoFormat("Transfer transaction completed count: {0}", currentCount);
            }
            if (currentCount == _expectedCount)
            {
                _waitHandle.Set();
            }
            return Task.CompletedTask;
        }
    }
}
