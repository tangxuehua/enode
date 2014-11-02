using System;
using BankTransferSample.Domain;
using ENode.Eventing;

namespace BankTransferSample.DomainEvents
{
    /// <summary>存款交易已开始
    /// </summary>
    [Serializable]
    public class DepositTransactionStartedEvent : DomainEvent<string>
    {
        public string AccountId { get; private set; }
        public double Amount { get; private set; }

        public DepositTransactionStartedEvent(string transactionId, string accountId, double amount)
            : base(transactionId)
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

        public DepositTransactionPreparationCompletedEvent(string transactionId, string accountId)
            : base(transactionId)
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

        public DepositTransactionCompletedEvent(string transactionId, string accountId)
            : base(transactionId)
        {
            AccountId = accountId;
        }
    }
}
