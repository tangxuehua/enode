using System;
using BankTransferSagaSample.Events;
using ENode.Domain;
using ENode.Eventing;

namespace BankTransferSagaSample.Domain
{
    [Serializable]
    public class TransferProcessManager : AggregateRoot<Guid>,
        IEventHandler<TransferProcessStarted>,
        IEventHandler<TransferOutRequested>,
        IEventHandler<TransferedOutHandled>,
        IEventHandler<TransferInRequested>,
        IEventHandler<TransferedInHandled>,
        IEventHandler<TransferProcessCompleted>,
        IEventHandler<TransferOutFailHandled>,
        IEventHandler<TransferInFailHandled>,
        IEventHandler<RollbackTransferOutRequested>
    {
        public TransferState CurrentTransferState { get; private set; }
        public ProcessState CurrentProcessState { get; private set; }
        public string ErrorMessage { get; private set; }

        public TransferProcessManager() : base() { }
        public TransferProcessManager(BankAccount sourceAccount, BankAccount targetAccount, TransferInfo transferInfo) : base(Guid.NewGuid())
        {
            RaiseEvent(new TransferProcessStarted(Id, transferInfo, string.Format("转账流程启动，源账户：{0}，目标账户：{1}，转账金额：{2}",
                        sourceAccount.AccountNumber,
                        targetAccount.AccountNumber,
                        transferInfo.Amount)));
            RaiseEvent(new TransferOutRequested(Id, transferInfo));
        }

        public void HandleTransferedOut(TransferInfo transferInfo)
        {
            RaiseEvent(new TransferedOutHandled(Id, transferInfo));
            RaiseEvent(new TransferInRequested(Id, transferInfo));
        }
        public void HandleTransferedIn(TransferInfo transferInfo)
        {
            RaiseEvent(new TransferedInHandled(Id, transferInfo));
            RaiseEvent(new TransferProcessCompleted(Id, transferInfo));
        }
        public void HandleTransferOutFail(TransferInfo transferInfo, string errorMessage)
        {
            RaiseEvent(new TransferOutFailHandled(Id, transferInfo, errorMessage));
        }
        public void HandleTransferInFail(TransferInfo transferInfo, string errorMessage)
        {
            RaiseEvent(new RollbackTransferOutRequested(Id, transferInfo));
            RaiseEvent(new TransferInFailHandled(Id, transferInfo, errorMessage));
        }
        public void Complete(TransferInfo transferInfo)
        {
            RaiseEvent(new TransferProcessCompleted(Id, transferInfo));
        }

        void IEventHandler<TransferProcessStarted>.Handle(TransferProcessStarted evnt)
        {
            CurrentProcessState = ProcessState.Started;
        }
        void IEventHandler<TransferOutRequested>.Handle(TransferOutRequested evnt)
        {
            CurrentProcessState = ProcessState.TransferOutRequested;
        }
        void IEventHandler<TransferedOutHandled>.Handle(TransferedOutHandled evnt)
        {
            CurrentTransferState = TransferState.TransferedOut;
        }
        void IEventHandler<TransferInRequested>.Handle(TransferInRequested evnt)
        {
            CurrentProcessState = ProcessState.TransferInRequested;
        }
        void IEventHandler<TransferedInHandled>.Handle(TransferedInHandled evnt)
        {
            CurrentTransferState = TransferState.TransferedIn;
        }
        void IEventHandler<TransferProcessCompleted>.Handle(TransferProcessCompleted evnt)
        {
            CurrentProcessState = ProcessState.Completed;
        }
        void IEventHandler<TransferOutFailHandled>.Handle(TransferOutFailHandled evnt)
        {
            CurrentProcessState = ProcessState.TransferOutFailed;
            ErrorMessage = evnt.ErrorMessage;
        }
        void IEventHandler<TransferInFailHandled>.Handle(TransferInFailHandled evnt)
        {
            CurrentProcessState = ProcessState.TransferInFailed;
            ErrorMessage = evnt.ErrorMessage;
        }
        void IEventHandler<RollbackTransferOutRequested>.Handle(RollbackTransferOutRequested evnt)
        {
            CurrentProcessState = ProcessState.RollbackTransferOutRequested;
        }

        public enum ProcessState
        {
            NotStarted,
            Started,
            TransferOutRequested,
            TransferInRequested,
            Completed,
            TransferOutFailed,
            TransferInFailed,
            RollbackTransferOutRequested
        }
        public enum TransferState
        {
            None,
            TransferedOut,
            TransferedIn
        }
    }

    public class TransferInfo
    {
        public Guid SourceAccountId { get; private set; }
        public Guid TargetAccountId { get; private set; }
        public double Amount { get; private set; }

        public TransferInfo(Guid sourceAccountId, Guid targetAccountId, double amount)
        {
            SourceAccountId = sourceAccountId;
            TargetAccountId = targetAccountId;
            Amount = amount;
        }
    }
}
