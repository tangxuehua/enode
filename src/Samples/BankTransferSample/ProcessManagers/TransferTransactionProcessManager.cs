using BankTransferSample.Commands;
using BankTransferSample.Domain;
using BankTransferSample.DomainEvents;
using ECommon.Components;
using ENode.Eventing;

namespace BankTransferSample.ProcessManagers
{
    /// <summary>银行转账交易流程管理器，用于协调银行转账交易流程中各个参与者聚合根之间的消息交互。
    /// </summary>
    [Component]
    public class TransferTransactionProcessManager :
        IEventHandler<TransferTransactionStartedEvent>,                  //转账交易已开始
        IEventHandler<TransactionPreparationAddedEvent>,                 //账户预操作已添加
        IEventHandler<TransferTransactionPreparationCompletedEvent>,     //转账交易预转出和预转入都已确认
        IEventHandler<TransactionPreparationCommittedEvent>,             //账户预操作已提交
        IEventHandler<InsufficientBalanceEvent>,                         //账户余额不足
        IEventHandler<TransferTransactionCancelStartedEvent>,            //转账交易取消已开始
        IEventHandler<TransactionPreparationCanceledEvent>               //账户预操作已取消
    {
        public void Handle(IEventContext context, TransferTransactionStartedEvent evnt)
        {
            context.AddCommand(new AddTransactionPreparationCommand(
                evnt.TransactionInfo.SourceAccountId,
                evnt.AggregateRootId,
                TransactionType.TransferTransaction,
                PreparationType.DebitPreparation,
                evnt.TransactionInfo.Amount));
            context.AddCommand(new AddTransactionPreparationCommand(
                evnt.TransactionInfo.TargetAccountId,
                evnt.AggregateRootId,
                TransactionType.TransferTransaction,
                PreparationType.CreditPreparation,
                evnt.TransactionInfo.Amount));
        }
        public void Handle(IEventContext context, TransactionPreparationAddedEvent evnt)
        {
            if (evnt.TransactionPreparation.TransactionType == TransactionType.TransferTransaction)
            {
                if (evnt.TransactionPreparation.PreparationType == PreparationType.DebitPreparation)
                {
                    context.AddCommand(new ConfirmTransferOutPreparationCommand(evnt.TransactionPreparation.TransactionId));
                }
                else if (evnt.TransactionPreparation.PreparationType == PreparationType.CreditPreparation)
                {
                    context.AddCommand(new ConfirmTransferInPreparationCommand(evnt.TransactionPreparation.TransactionId));
                }
            }
        }
        public void Handle(IEventContext context, TransferTransactionPreparationCompletedEvent evnt)
        {
            context.AddCommand(new CommitTransactionPreparationCommand(evnt.TransactionInfo.SourceAccountId, evnt.AggregateRootId));
            context.AddCommand(new CommitTransactionPreparationCommand(evnt.TransactionInfo.TargetAccountId, evnt.AggregateRootId));
        }
        public void Handle(IEventContext context, TransactionPreparationCommittedEvent evnt)
        {
            if (evnt.TransactionPreparation.TransactionType == TransactionType.TransferTransaction)
            {
                if (evnt.TransactionPreparation.PreparationType == PreparationType.DebitPreparation)
                {
                    context.AddCommand(new ConfirmTransferOutCommand(evnt.TransactionPreparation.TransactionId));
                }
                else if (evnt.TransactionPreparation.PreparationType == PreparationType.CreditPreparation)
                {
                    context.AddCommand(new ConfirmTransferInCommand(evnt.TransactionPreparation.TransactionId));
                }
            }
        }
        public void Handle(IEventContext context, InsufficientBalanceEvent evnt)
        {
            if (evnt.TransactionType == TransactionType.TransferTransaction)
            {
                context.AddCommand(new StartCancelTransferTransactionCommand(evnt.TransactionId));
            }
        }
        public void Handle(IEventContext context, TransferTransactionCancelStartedEvent evnt)
        {
            context.AddCommand(new CancelTransactionPreparationCommand(evnt.TransactionInfo.SourceAccountId, evnt.AggregateRootId));
            context.AddCommand(new CancelTransactionPreparationCommand(evnt.TransactionInfo.TargetAccountId, evnt.AggregateRootId));
        }
        public void Handle(IEventContext context, TransactionPreparationCanceledEvent evnt)
        {
            if (evnt.TransactionPreparation.TransactionType == TransactionType.TransferTransaction)
            {
                if (evnt.TransactionPreparation.PreparationType == PreparationType.DebitPreparation)
                {
                    context.AddCommand(new ConfirmTransferOutCanceledCommand(evnt.TransactionPreparation.TransactionId));
                }
                else if (evnt.TransactionPreparation.PreparationType == PreparationType.CreditPreparation)
                {
                    context.AddCommand(new ConfirmTransferInCanceledCommand(evnt.TransactionPreparation.TransactionId));
                }
            }
        }
    }
}
