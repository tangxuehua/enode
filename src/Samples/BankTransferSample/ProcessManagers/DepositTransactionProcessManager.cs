using BankTransferSample.Commands;
using BankTransferSample.Domain;
using BankTransferSample.DomainEvents;
using ECommon.Components;
using ENode.Eventing;

namespace BankTransferSample.ProcessManagers
{
    /// <summary>银行存款交易流程管理器，用于协调银行存款交易流程中各个参与者聚合根之间的消息交互。
    /// </summary>
    [Component]
    public class DepositTransactionProcessManager :
        IEventHandler<DepositTransactionStartedEvent>,                    //存款交易已开始
        IEventHandler<DepositTransactionPreparationCompletedEvent>,                  //存款交易已提交
        IEventHandler<TransactionPreparationAddedEvent>,                  //账户预操作已添加
        IEventHandler<TransactionPreparationCommittedEvent>               //账户预操作已提交
    {
        public void Handle(IEventContext context, DepositTransactionStartedEvent evnt)
        {
            context.AddCommand(new AddTransactionPreparationCommand(
                evnt.AccountId,
                evnt.AggregateRootId,
                TransactionType.DepositTransaction,
                PreparationType.CreditPreparation,
                evnt.Amount));
        }
        public void Handle(IEventContext context, TransactionPreparationAddedEvent evnt)
        {
            if (evnt.TransactionPreparation.TransactionType == TransactionType.DepositTransaction &&
                evnt.TransactionPreparation.PreparationType == PreparationType.CreditPreparation)
            {
                context.AddCommand(new ConfirmDepositPreparationCommand(evnt.TransactionPreparation.TransactionId));
            }
        }
        public void Handle(IEventContext context, DepositTransactionPreparationCompletedEvent evnt)
        {
            context.AddCommand(new CommitTransactionPreparationCommand(evnt.AccountId, evnt.AggregateRootId));
        }
        public void Handle(IEventContext context, TransactionPreparationCommittedEvent evnt)
        {
            if (evnt.TransactionPreparation.TransactionType == TransactionType.DepositTransaction &&
                evnt.TransactionPreparation.PreparationType == PreparationType.CreditPreparation)
            {
                context.AddCommand(new ConfirmDepositCommand(evnt.TransactionPreparation.TransactionId));
            }
        }
    }
}
