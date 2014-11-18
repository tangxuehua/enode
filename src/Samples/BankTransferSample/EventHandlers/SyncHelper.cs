using System.Threading;
using BankTransferSample.DomainEvents;
using ECommon.Components;
using ENode.Eventing;
using ENode.Infrastructure;

namespace BankTransferSample.EventHandlers
{
    [Component]
    public class SyncHelper :
        IEventHandler<DepositTransactionCompletedEvent>,
        IEventHandler<TransferTransactionCompletedEvent>,
        IEventHandler<TransferTransactionCanceledEvent>
    {
        private ManualResetEvent _waitHandle = new ManualResetEvent(false);

        public void WaitOne()
        {
            _waitHandle.WaitOne();
        }

        public void Handle(IHandlingContext context, DepositTransactionCompletedEvent message)
        {
            _waitHandle.Set();
            _waitHandle = new ManualResetEvent(false);
        }
        public void Handle(IHandlingContext context, TransferTransactionCompletedEvent message)
        {
            _waitHandle.Set();
            _waitHandle = new ManualResetEvent(false);
        }
        public void Handle(IHandlingContext context, TransferTransactionCanceledEvent message)
        {
            _waitHandle.Set();
            _waitHandle = new ManualResetEvent(false);
        }
    }
}
