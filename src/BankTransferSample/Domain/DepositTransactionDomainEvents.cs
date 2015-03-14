using System;
using BankTransferSample.Domain;
using ENode.Eventing;

namespace BankTransferSample.Domain
{
    /// <summary>存款交易已开始
    /// </summary>
    [Serializable]
    public class DepositTransactionStartedEvent : DomainEvent<string>
    {
        public string AccountId { get; private set; }
        public double Amount { get; private set; }

        public DepositTransactionStartedEvent() { }
        public DepositTransactionStartedEvent(DepositTransaction transaction, string accountId, double amount)
            : base(transaction)
        {
            AccountId = accountId;
            Amount = amount;
        }
    }
    /// <summary>存款交易预存款已确认
    /// </summary>
    [Serializable]
    public class DepositTransactionPreparationCompletedEvent : DomainEvent<string>
    {
        public string AccountId { get; private set; }

        public DepositTransactionPreparationCompletedEvent() { }
        public DepositTransactionPreparationCompletedEvent(DepositTransaction transaction, string accountId)
            : base(transaction)
        {
            AccountId = accountId;
        }
    }
    /// <summary>存款交易已完成
    /// </summary>
    [Serializable]
    public class DepositTransactionCompletedEvent : DomainEvent<string>
    {
        public string AccountId { get; private set; }

        public DepositTransactionCompletedEvent() { }
        public DepositTransactionCompletedEvent(DepositTransaction transaction, string accountId)
            : base(transaction)
        {
            AccountId = accountId;
        }
    }
}
