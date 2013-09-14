using System;
using BankTransferSagaSample.Events;
using ENode.Domain;
using ENode.Eventing;
using ENode.Infrastructure;

namespace BankTransferSagaSample.Domain
{
    /// <summary>银行转账流程聚合根
    /// <remarks>
    /// 负责封装转账流程的当前状态以及流程下一步该怎么走的逻辑，包括遇到异常时的回滚处理逻辑。
    /// </remarks>
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
        public TransferProcessState State { get; private set; }

        public TransferProcess() { }
        public TransferProcess(Guid processId, TransferInfo transferInfo) : base(processId)
        {
            RaiseEvent(new TransferProcessStarted(Id, transferInfo, string.Format("转账流程启动，源账户：{0}，目标账户：{1}，转账金额：{2}", transferInfo.SourceAccountNumber, transferInfo.TargetAccountNumber, transferInfo.Amount)));
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
        /// <param name="errorInfo"></param>
        public void HandleFailedTransferOut(TransferInfo transferInfo, ErrorInfo errorInfo)
        {
            RaiseEvent(new TransferProcessCompleted(Id, transferInfo, new TransferProcessResult(false, errorInfo)));
        }
        /// <summary>处理转入失败的情况
        /// </summary>
        /// <param name="transferInfo"></param>
        /// <param name="errorInfo"></param>
        public void HandleFailedTransferIn(TransferInfo transferInfo, ErrorInfo errorInfo)
        {
            RaiseEvent(new RollbackTransferOutRequested(Id, transferInfo, errorInfo));
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
            State = TransferProcessState.Started;
        }
        void IEventHandler<TransferOutRequested>.Handle(TransferOutRequested evnt)
        {
            State = TransferProcessState.TransferOutRequested;
        }
        void IEventHandler<TransferInRequested>.Handle(TransferInRequested evnt)
        {
            State = TransferProcessState.TransferInRequested;
        }
        void IEventHandler<RollbackTransferOutRequested>.Handle(RollbackTransferOutRequested evnt)
        {
            State = TransferProcessState.RollbackTransferOutRequested;
            Result = new TransferProcessResult(false, evnt.ErrorInfo);
        }
        void IEventHandler<TransferProcessCompleted>.Handle(TransferProcessCompleted evnt)
        {
            State = TransferProcessState.Completed;
            Result = evnt.ProcessResult;
        }
    }
}
