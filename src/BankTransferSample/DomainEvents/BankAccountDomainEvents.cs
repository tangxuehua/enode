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

        public AccountCreatedEvent() { }
        public AccountCreatedEvent(string accountId, string owner)
            : base(accountId)
        {
            Owner = owner;
        }
    }
    /// <summary>账户验证已通过
    /// </summary>
    [Serializable]
    public class AccountValidatePassedEvent : Event
    {
        public string AccountId { get; set; }
        public string TransactionId { get; set; }

        public AccountValidatePassedEvent() { }
        public AccountValidatePassedEvent(string accountId, string transactionId)
        {
            AccountId = accountId;
            TransactionId = transactionId;
        }
    }
    /// <summary>账户预操作已添加
    /// </summary>
    [Serializable]
    public class TransactionPreparationAddedEvent : DomainEvent<string>
    {
        public TransactionPreparation TransactionPreparation { get; private set; }

        public TransactionPreparationAddedEvent() { }
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

        public TransactionPreparationCommittedEvent() { }
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

        public TransactionPreparationCanceledEvent() { }
        public TransactionPreparationCanceledEvent(TransactionPreparation transactionPreparation)
            : base(transactionPreparation.AccountId)
        {
            TransactionPreparation = transactionPreparation;
        }
    }
}
