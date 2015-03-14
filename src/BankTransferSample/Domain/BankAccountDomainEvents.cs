using System;
using BankTransferSample.Domain;
using ENode.Eventing;
using ENode.Infrastructure;

namespace BankTransferSample.Domain
{
    /// <summary>已开户
    /// </summary>
    [Serializable]
    public class AccountCreatedEvent : DomainEvent<string>
    {
        /// <summary>账户拥有者
        /// </summary>
        public string Owner { get; private set; }

        public AccountCreatedEvent() { }
        public AccountCreatedEvent(BankAccount account, string owner)
            : base(account)
        {
            Owner = owner;
        }
    }
    /// <summary>账户预操作已添加
    /// </summary>
    [Serializable]
    public class TransactionPreparationAddedEvent : DomainEvent<string>
    {
        public TransactionPreparation TransactionPreparation { get; private set; }

        public TransactionPreparationAddedEvent() { }
        public TransactionPreparationAddedEvent(BankAccount account, TransactionPreparation transactionPreparation)
            : base(account)
        {
            TransactionPreparation = transactionPreparation;
        }
    }
    /// <summary>账户预操作已执行
    /// </summary>
    [Serializable]
    public class TransactionPreparationCommittedEvent : DomainEvent<string>
    {
        public double CurrentBalance { get; private set; }
        public TransactionPreparation TransactionPreparation { get; private set; }

        public TransactionPreparationCommittedEvent() { }
        public TransactionPreparationCommittedEvent(BankAccount account, double currentBalance, TransactionPreparation transactionPreparation)
            : base(account)
        {
            CurrentBalance = currentBalance;
            TransactionPreparation = transactionPreparation;
        }
    }
    /// <summary>账户预操作已取消
    /// </summary>
    [Serializable]
    public class TransactionPreparationCanceledEvent : DomainEvent<string>
    {
        public TransactionPreparation TransactionPreparation { get; private set; }

        public TransactionPreparationCanceledEvent() { }
        public TransactionPreparationCanceledEvent(BankAccount account, TransactionPreparation transactionPreparation)
            : base(account)
        {
            TransactionPreparation = transactionPreparation;
        }
    }
}
