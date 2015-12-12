using System;
using BankTransferSample.Domain;
using ENode.Eventing;
using ENode.Infrastructure;

namespace BankTransferSample.Domain
{
    /// <summary>存款交易已开始
    /// </summary>
    public class DepositTransactionStartedEvent : DomainEvent<string>
    {
        public string AccountId { get; private set; }
        public double Amount { get; private set; }

        public DepositTransactionStartedEvent() { }
        public DepositTransactionStartedEvent(string accountId, double amount)
        {
            AccountId = accountId;
            Amount = amount;
        }
    }
    /// <summary>存款交易预存款已确认
    /// </summary>
    public class DepositTransactionPreparationCompletedEvent : DomainEvent<string>
    {
        public string AccountId { get; private set; }

        public DepositTransactionPreparationCompletedEvent() { }
        public DepositTransactionPreparationCompletedEvent(string accountId)
        {
            AccountId = accountId;
        }
    }
    /// <summary>存款交易已完成
    /// </summary>
    public class DepositTransactionCompletedEvent : DomainEvent<string>
    {
        public string AccountId { get; private set; }

        public DepositTransactionCompletedEvent() { }
        public DepositTransactionCompletedEvent(string accountId)
        {
            AccountId = accountId;
        }
    }
}
