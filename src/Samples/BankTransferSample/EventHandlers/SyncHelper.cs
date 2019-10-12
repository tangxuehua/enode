using System.Threading;
using System.Threading.Tasks;
using BankTransferSample.Domain;
using ENode.Messaging;

namespace BankTransferSample.EventHandlers
{
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

        public Task HandleAsync(DepositTransactionCompletedEvent message)
        {
            _waitHandle.Set();
            _waitHandle = new ManualResetEvent(false);
            return Task.CompletedTask;
        }
        public Task HandleAsync(TransferTransactionCompletedEvent message)
        {
            _waitHandle.Set();
            _waitHandle = new ManualResetEvent(false);
            return Task.CompletedTask;
        }
        public Task HandleAsync(TransferTransactionCanceledEvent message)
        {
            _waitHandle.Set();
            _waitHandle = new ManualResetEvent(false);
            return Task.CompletedTask;
        }
    }
}
