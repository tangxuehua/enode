using System.Threading;
using System.Threading.Tasks;
using BankTransferSample.Domain;
using ECommon.Components;
using ECommon.IO;
using ENode.Eventing;
using ENode.Infrastructure;

namespace BankTransferSample.EventHandlers
{
    [Component]
    [Code(10002)]
    public class SyncHelper :
        IMessageHandler<DepositTransactionCompletedEvent>,
        IMessageHandler<TransferTransactionCompletedEvent>,
        IMessageHandler<TransferTransactionCanceledEvent>
    {
        private ManualResetEvent _waitHandle = new ManualResetEvent(false);

        public void WaitOne()
        {
            _waitHandle.WaitOne();
        }

        public Task<AsyncTaskResult> HandleAsync(DepositTransactionCompletedEvent message)
        {
            _waitHandle.Set();
            _waitHandle = new ManualResetEvent(false);
            return Task.FromResult(AsyncTaskResult.Success);
        }
        public Task<AsyncTaskResult> HandleAsync(TransferTransactionCompletedEvent message)
        {
            _waitHandle.Set();
            _waitHandle = new ManualResetEvent(false);
            return Task.FromResult(AsyncTaskResult.Success);
        }
        public Task<AsyncTaskResult> HandleAsync(TransferTransactionCanceledEvent message)
        {
            _waitHandle.Set();
            _waitHandle = new ManualResetEvent(false);
            return Task.FromResult(AsyncTaskResult.Success);
        }
    }
}
