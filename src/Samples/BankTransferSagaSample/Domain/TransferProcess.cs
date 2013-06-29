using System;
using BankTransferSagaSample.Events;
using ENode.Domain;
using ENode.Eventing;

namespace BankTransferSagaSample.Domain
{
    /// <summary>转账流程状态
    /// </summary>
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
    /// <summary>转账信息值对象，包含了转账的基本信息
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
    /// <summary>银行转账流程聚合根，负责控制整个转账的过程，包括遇到异常时的回滚处理
    /// </summary>
    [Serializable]
    public class TransferProcess : Process<Guid>,
        IEventHandler<TransferProcessStarted>,       //转账流程已开始
        IEventHandler<TransferOutRequested>,         //转出的请求已发起
        IEventHandler<TransferInRequested>,          //转入的请求已发起
        IEventHandler<RollbackTransferOutRequested>, //回滚转出的请求已发起
        IEventHandler<TransferProcessCompleted>      //转账流程已完成
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

        /// <summary>处理已转出事件
        /// </summary>
        /// <param name="transferInfo"></param>
        public void HandleTransferedOut(TransferInfo transferInfo)
        {
            RaiseEvent(new TransferInRequested(Id, transferInfo));
        }
        /// <summary>处理已转入事件
        /// </summary>
        /// <param name="transferInfo"></param>
        public void HandleTransferedIn(TransferInfo transferInfo)
        {
            RaiseEvent(new TransferProcessCompleted(Id, transferInfo, ProcessResult.Success));
        }
        /// <summary>处理转出失败的情况
        /// </summary>
        /// <param name="transferInfo"></param>
        public void HandleFailedTransferOut(TransferInfo transferInfo, string errorMessage)
        {
            RaiseEvent(new TransferProcessCompleted(Id, transferInfo, new ProcessResult(false, errorMessage)));
        }
        /// <summary>处理转入失败的情况
        /// </summary>
        /// <param name="transferInfo"></param>
        public void HandleFailedTransferIn(TransferInfo transferInfo, string errorMessage)
        {
            RaiseEvent(new RollbackTransferOutRequested(Id, transferInfo, errorMessage));
        }
        /// <summary>处理转出已回滚事件
        /// </summary>
        /// <param name="transferInfo"></param>
        public void HandleTransferOutRolledback(TransferInfo transferInfo)
        {
            RaiseEvent(new TransferProcessCompleted(Id, transferInfo, Result));
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
            Result = new ProcessResult(false, evnt.ErrorMessage);
        }
        void IEventHandler<TransferProcessCompleted>.Handle(TransferProcessCompleted evnt)
        {
            State = ProcessState.Completed;
            IsCompleted = true;
            Result = evnt.ProcessResult;
        }
    }
}
