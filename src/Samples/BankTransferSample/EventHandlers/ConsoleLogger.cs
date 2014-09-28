using System;
using BankTransferSample.Domain;
using BankTransferSample.DomainEvents;
using ECommon.Components;
using ENode.Eventing;

namespace BankTransferSample.EventHandlers
{
    [Component]
    public class ConsoleLogger :
        IEventHandler<AccountCreatedEvent>,
        IEventHandler<AccountValidatePassedEvent>,
        IEventHandler<TransactionPreparationAddedEvent>,
        IEventHandler<TransactionPreparationCommittedEvent>,
        IEventHandler<TransferTransactionStartedEvent>,
        IEventHandler<TransferOutPreparationConfirmedEvent>,
        IEventHandler<TransferInPreparationConfirmedEvent>,
        IEventHandler<TransferTransactionCompletedEvent>,
        IEventHandler<InsufficientBalanceEvent>,
        IEventHandler<TransferTransactionCanceledEvent>
    {
        public void Handle(IEventContext context, AccountCreatedEvent evnt)
        {
            Console.WriteLine("账户已创建，账户：{0}，所有者：{1}", evnt.AggregateRootId, evnt.Owner);
        }
        public void Handle(IEventContext context, AccountValidatePassedEvent evnt)
        {
            Console.WriteLine("账户验证已通过，交易ID：{0}，账户：{1}", evnt.TransactionId, evnt.AccountId);
        }
        public void Handle(IEventContext context, TransactionPreparationAddedEvent evnt)
        {
            if (evnt.TransactionPreparation.TransactionType == TransactionType.TransferTransaction)
            {
                if (evnt.TransactionPreparation.PreparationType == PreparationType.DebitPreparation)
                {
                    Console.WriteLine("账户预转出成功，交易ID：{0}，账户：{1}，金额：{2}", evnt.TransactionPreparation.TransactionId, evnt.TransactionPreparation.AccountId, evnt.TransactionPreparation.Amount);
                }
                else if (evnt.TransactionPreparation.PreparationType == PreparationType.CreditPreparation)
                {
                    Console.WriteLine("账户预转入成功，交易ID：{0}，账户：{1}，金额：{2}", evnt.TransactionPreparation.TransactionId, evnt.TransactionPreparation.AccountId, evnt.TransactionPreparation.Amount);
                }
            }
        }
        public void Handle(IEventContext context, TransactionPreparationCommittedEvent evnt)
        {
            if (evnt.TransactionPreparation.TransactionType == TransactionType.DepositTransaction)
            {
                if (evnt.TransactionPreparation.PreparationType == PreparationType.CreditPreparation)
                {
                    Console.WriteLine("账户存款已成功，账户：{0}，金额：{1}，当前余额：{2}", evnt.TransactionPreparation.AccountId, evnt.TransactionPreparation.Amount, evnt.CurrentBalance);
                }
            }
            if (evnt.TransactionPreparation.TransactionType == TransactionType.TransferTransaction)
            {
                if (evnt.TransactionPreparation.PreparationType == PreparationType.DebitPreparation)
                {
                    Console.WriteLine("账户转出已成功，交易ID：{0}，账户：{1}，金额：{2}，当前余额：{3}", evnt.TransactionPreparation.TransactionId, evnt.TransactionPreparation.AccountId, evnt.TransactionPreparation.Amount, evnt.CurrentBalance);
                }
                if (evnt.TransactionPreparation.PreparationType == PreparationType.CreditPreparation)
                {
                    Console.WriteLine("账户转入已成功，交易ID：{0}，账户：{1}，金额：{2}，当前余额：{3}", evnt.TransactionPreparation.TransactionId, evnt.TransactionPreparation.AccountId, evnt.TransactionPreparation.Amount, evnt.CurrentBalance);
                }
            }
        }

        public void Handle(IEventContext context, TransferTransactionStartedEvent evnt)
        {
            Console.WriteLine("转账交易已开始，交易ID：{0}，源账户：{1}，目标账户：{2}，转账金额：{3}", evnt.AggregateRootId, evnt.TransactionInfo.SourceAccountId, evnt.TransactionInfo.TargetAccountId, evnt.TransactionInfo.Amount);
        }
        public void Handle(IEventContext context, TransferOutPreparationConfirmedEvent evnt)
        {
            Console.WriteLine("预转出确认成功，交易ID：{0}，账户：{1}", evnt.AggregateRootId, evnt.TransactionInfo.SourceAccountId);
        }
        public void Handle(IEventContext context, TransferInPreparationConfirmedEvent evnt)
        {
            Console.WriteLine("预转入确认成功，交易ID：{0}，账户：{1}", evnt.AggregateRootId, evnt.TransactionInfo.TargetAccountId);
        }
        public void Handle(IEventContext context, TransferTransactionCompletedEvent evnt)
        {
            Console.WriteLine("转账交易已完成，交易ID：{0}", evnt.AggregateRootId);
        }

        public void Handle(IEventContext context, InsufficientBalanceEvent evnt)
        {
            Console.WriteLine("账户的余额不足，交易ID：{0}，账号：{1}，可用余额：{2}，转出金额：{3}", evnt.TransactionId, evnt.AggregateRootId, evnt.CurrentAvailableBalance, evnt.Amount);
        }
        public void Handle(IEventContext context, TransferTransactionCanceledEvent evnt)
        {
            Console.WriteLine("转账交易已取消，交易ID：{0}", evnt.AggregateRootId);
        }
    }
}
