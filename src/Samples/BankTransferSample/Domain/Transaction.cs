using System;
using BankTransferSample.Events;
using ENode.Domain;
using ENode.Eventing;
using ENode.Infrastructure;

namespace BankTransferSample.Domain
{
    /// <summary>银行转账交易聚合根，封装一次转账交易的数据一致性
    /// </summary>
    [Serializable]
    public class Transaction : AggregateRoot<Guid>
    {
        /// <summary>转账流程结果
        /// </summary>
        public TransactionResult Result { get; protected set; }
        /// <summary>当前转账流程状态
        /// </summary>
        public TransferProcessState State { get; private set; }

        public Transaction(Guid processId, TransferInfo transferInfo) : base(processId)
        {
            RaiseEvent(new TransferProcessStarted(Id, transferInfo, string.Format("转账流程启动，源账户：{0}，目标账户：{1}，转账金额：{2}", transferInfo.SourceAccountId, transferInfo.TargetAccountId, transferInfo.Amount)));
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
            RaiseEvent(new TransferProcessCompleted(Id, transferInfo, TransactionResult.Success));
        }
        /// <summary>处理转出失败的情况
        /// </summary>
        /// <param name="transferInfo"></param>
        /// <param name="errorInfo"></param>
        public void HandleFailedTransferOut(TransferInfo transferInfo, ErrorInfo errorInfo)
        {
            RaiseEvent(new TransferProcessCompleted(Id, transferInfo, new TransactionResult(false, errorInfo)));
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

        private void Handle(TransferProcessStarted evnt)
        {
            State = TransferProcessState.Started;
        }
        private void Handle(TransferOutRequested evnt)
        {
            State = TransferProcessState.TransferOutRequested;
        }
        private void Handle(TransferInRequested evnt)
        {
            State = TransferProcessState.TransferInRequested;
        }
        private void Handle(RollbackTransferOutRequested evnt)
        {
            State = TransferProcessState.RollbackTransferOutRequested;
            Result = new TransactionResult(false, evnt.ErrorInfo);
        }
        private void Handle(TransferProcessCompleted evnt)
        {
            State = TransferProcessState.Completed;
            Result = evnt.ProcessResult;
        }
    }
}
