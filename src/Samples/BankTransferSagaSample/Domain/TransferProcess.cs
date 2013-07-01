using System;
using BankTransferSagaSample.Events;
using ENode.Domain;
using ENode.Eventing;
using ENode.Infrastructure;

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
        Completed
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
    /// <summary>值对象，包含了转账流程的结果信息
    /// </summary>
    [Serializable]
    public class TransferProcessResult
    {
        private static readonly TransferProcessResult _successResult = new TransferProcessResult(true, null, null);

        /// <summary>转账是否成功
        /// </summary>
        public bool IsSuccess { get; private set; }
        /// <summary>错误信息
        /// </summary>
        public string ErrorMessage { get; private set; }
        /// <summary>异常
        /// </summary>
        public Exception Exception { get; private set; }

        public TransferProcessResult(bool isSuccess, string errorMessage, Exception exception)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
            Exception = exception;
        }

        /// <summary>表示转账成功的结果
        /// </summary>
        public static TransferProcessResult Success { get { return _successResult; } }
    }
    /// <summary>银行转账流程聚合根，负责控制整个转账的过程，包括遇到异常时的回滚处理
    /// </summary>
    [Serializable]
    public class TransferProcess : AggregateRoot<Guid>,
        IEventHandler<TransferProcessStarted>,       //转账流程已开始
        IEventHandler<TransferOutRequested>,         //转出的请求已发起
        IEventHandler<TransferInRequested>,          //转入的请求已发起
        IEventHandler<RollbackTransferOutRequested>, //回滚转出的请求已发起
        IEventHandler<TransferProcessCompleted>      //转账流程已完成
    {
        /// <summary>转账流程结果
        /// </summary>
        public TransferProcessResult Result { get; protected set; }
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
            RaiseEvent(new TransferProcessCompleted(Id, transferInfo, TransferProcessResult.Success));
        }
        /// <summary>处理转出失败的情况
        /// </summary>
        /// <param name="transferInfo"></param>
        public void HandleFailedTransferOut(TransferInfo transferInfo, Exception exception)
        {
            RaiseEvent(new TransferProcessCompleted(Id, transferInfo, new TransferProcessResult(false, exception.Message, exception)));
        }
        /// <summary>处理转入失败的情况
        /// </summary>
        /// <param name="transferInfo"></param>
        public void HandleFailedTransferIn(TransferInfo transferInfo, Exception exception)
        {
            RaiseEvent(new RollbackTransferOutRequested(Id, transferInfo, exception));
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
            Result = new TransferProcessResult(false, evnt.ProcessException.Message, evnt.ProcessException);
        }
        void IEventHandler<TransferProcessCompleted>.Handle(TransferProcessCompleted evnt)
        {
            State = ProcessState.Completed;
            Result = evnt.ProcessResult;
        }
    }
}
