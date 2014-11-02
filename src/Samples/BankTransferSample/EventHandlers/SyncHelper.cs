using System.Threading;
using BankTransferSample.DomainEvents;
using ECommon.Components;
using ENode.Eventing;

namespace BankTransferSample.EventHandlers
{
    [Component(LifeStyle.Singleton)]
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

        public void Handle(IEventContext context, DepositTransactionCompletedEvent message)
        {
            _waitHandle.Set();
            _waitHandle = new ManualResetEvent(false);
        }
        public void Handle(IEventContext context, TransferTransactionCompletedEvent message)
        {
            _waitHandle.Set();
            _waitHandle = new ManualResetEvent(false);
        }
        public void Handle(IEventContext context, TransferTransactionCanceledEvent message)
        {
            _waitHandle.Set();
            _waitHandle = new ManualResetEvent(false);
        }
    }
}
