using System;
using BankTransferSample.Domain;
using ENode.Eventing;
using ENode.Infrastructure;

namespace BankTransferSample.Domain
{
    /// <summary>已开户
    /// </summary>
    public class AccountCreatedEvent : DomainEvent<string>
    {
        /// <summary>账户拥有者
        /// </summary>
        public string Owner { get; private set; }

        public AccountCreatedEvent() { }
        public AccountCreatedEvent(string owner)
        {
            Owner = owner;
        }
    }
    /// <summary>账户预操作已添加
    /// </summary>
    public class TransactionPreparationAddedEvent : DomainEvent<string>
    {
        public TransactionPreparation TransactionPreparation { get; private set; }

        public TransactionPreparationAddedEvent() { }
        public TransactionPreparationAddedEvent(TransactionPreparation transactionPreparation)
        {
            TransactionPreparation = transactionPreparation;
        }
    }
    /// <summary>账户预操作已执行
    /// </summary>
    public class TransactionPreparationCommittedEvent : DomainEvent<string>
    {
        public double CurrentBalance { get; private set; }
        public TransactionPreparation TransactionPreparation { get; private set; }

        public TransactionPreparationCommittedEvent() { }
        public TransactionPreparationCommittedEvent(double currentBalance, TransactionPreparation transactionPreparation)
        {
            CurrentBalance = currentBalance;
            TransactionPreparation = transactionPreparation;
        }
    }
    /// <summary>账户预操作已取消
    /// </summary>
    public class TransactionPreparationCanceledEvent : DomainEvent<string>
    {
        public TransactionPreparation TransactionPreparation { get; private set; }

        public TransactionPreparationCanceledEvent() { }
        public TransactionPreparationCanceledEvent(TransactionPreparation transactionPreparation)
        {
            TransactionPreparation = transactionPreparation;
        }
    }
}
