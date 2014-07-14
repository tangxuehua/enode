using System;
using BankTransferSample.Domain;
using ENode.Eventing;

namespace BankTransferSample.DomainEvents
{
    /// <summary>已开户
    /// </summary>
    [Serializable]
    public class AccountCreatedEvent : DomainEvent<string>
    {
        /// <summary>账户拥有者
        /// </summary>
        public string Owner { get; private set; }

        public AccountCreatedEvent(string accountId, string owner)
            : base(accountId)
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

        public TransactionPreparationAddedEvent(TransactionPreparation transactionPreparation)
            : base(transactionPreparation.AccountId)
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

        public TransactionPreparationCommittedEvent(double currentBalance, TransactionPreparation transactionPreparation)
            : base(transactionPreparation.AccountId)
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

        public TransactionPreparationCanceledEvent(TransactionPreparation transactionPreparation)
            : base(transactionPreparation.AccountId)
        {
            TransactionPreparation = transactionPreparation;
        }
    }
    /// <summary>余额不足，该领域事件不会改变账户的状态
    /// </summary>
    [Serializable]
    public class InsufficientBalanceEvent : DomainEvent<string>
    {
        /// <summary>交易ID
        /// </summary>
        public string TransactionId { get; private set; }
        /// <summary>交易类型
        /// </summary>
        public TransactionType TransactionType { get; private set; }
        /// <summary>预借金额
        /// </summary>
        public double Amount { get; private set; }
        /// <summary>当前余额
        /// </summary>
        public double CurrentBalance { get; private set; }
        /// <summary>当前可用余额
        /// </summary>
        public double CurrentAvailableBalance { get; private set; }

        public InsufficientBalanceEvent(string accountId, string transactionId, TransactionType transactionType, double amount, double currentBalance, double currentAvailableBalance)
            : base(accountId)
        {
            TransactionId = transactionId;
            TransactionType = transactionType;
            Amount = amount;
            CurrentBalance = currentBalance;
            CurrentAvailableBalance = currentAvailableBalance;
        }
    }
}
