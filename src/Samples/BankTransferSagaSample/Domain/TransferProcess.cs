using System;
using BankTransferSagaSample.Events;
using ENode.Domain;
using ENode.Eventing;

namespace BankTransferSagaSample.Domain
{
    /// <summary>银行转账流程聚合根，负责控制整个转账的过程，包括遇到异常时的回滚处理
    /// </summary>
    [Serializable]
    public class TransferProcess : AggregateRoot<Guid>,
        IEventHandler<TransferProcessStarted>,       //转账流程已开始
        IEventHandler<TransferOutRequested>,         //转出的请求已发起
        IEventHandler<TransferInRequested>,          //转入的请求已发起
        IEventHandler<RollbackTransferOutRequested>, //回滚转出的请求已发起
        IEventHandler<TransferProcessCompleted>,     //转账流程已正常完成
        IEventHandler<TransferProcessAborted>        //转账流程已异常终止
    {
        /// <summary>当前转账流程状态
        /// </summary>
        public ProcessState State { get; private set; }

        public TransferProcess() : base() { }
        public TransferProcess(BankAccount sourceAccount, BankAccount targetAccount, TransferInfo transferInfo) : base(Guid.NewGuid())
        {
            RaiseEvent(new TransferProcessStarted(Id, transferInfo, string.Format("转账流程启动，源账户：{0}，目标账户：{1}，转账金额：{2}",
                        sourceAccount.AccountNumber,
                        targetAccount.AccountNumber,
                        transferInfo.Amount)));
            RaiseEvent(new TransferOutRequested(Id, transferInfo));
        }

        public void HandleTransferedOut(TransferInfo transferInfo)
        {
            RaiseEvent(new TransferInRequested(Id, transferInfo));
        }
        public void HandleTransferedIn(TransferInfo transferInfo)
        {
            RaiseEvent(new TransferProcessCompleted(Id, transferInfo));
        }
        public void HandleFailedTransferOut(TransferInfo transferInfo)
        {
            RaiseEvent(new TransferProcessAborted(Id, transferInfo));
        }
        public void HandleFailedTransferIn(TransferInfo transferInfo)
        {
            RaiseEvent(new RollbackTransferOutRequested(Id, transferInfo));
        }
        public void HandleTransferOutRolledback(TransferInfo transferInfo)
        {
            RaiseEvent(new TransferProcessAborted(Id, transferInfo));
        }

        void IEventHandler<TransferProcessStarted>.Handle(TransferProcessStarted evnt)
        {
            State = ProcessState.Started;
        }
        void IEventHandler<TransferOutRequested>.Handle(TransferOutRequested evnt)
        {
            State = ProcessState.TransferOutRequested;
        }
        void IEventHandler<TransferInRequested>.Handle(TransferInRequested evnt)
        {
            State = ProcessState.TransferInRequested;
        }
        void IEventHandler<RollbackTransferOutRequested>.Handle(RollbackTransferOutRequested evnt)
        {
            State = ProcessState.RollbackTransferOutRequested;
        }
        void IEventHandler<TransferProcessCompleted>.Handle(TransferProcessCompleted evnt)
        {
            State = ProcessState.Completed;
        }
        void IEventHandler<TransferProcessAborted>.Handle(TransferProcessAborted evnt)
        {
            State = ProcessState.Aborted;
        }
    }
    public enum ProcessState
    {
        NotStarted,
        Started,
        TransferOutRequested,
        TransferInRequested,
        RollbackTransferOutRequested,
        Completed,
        Aborted
    }

    /// <summary>转账信息值对象
    /// </summary>
    [Serializable]
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
