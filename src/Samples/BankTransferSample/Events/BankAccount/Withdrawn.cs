using System;
using ENode.Eventing;

namespace BankTransferSample.Events
{
    /// <summary>已取款
    /// </summary>
    [Serializable]
    public class Withdrawn : DomainEvent<string>, ISourcingEvent
    {
        /// <summary>取款金额
        /// </summary>
        public double Amount { get; private set; }
        /// <summary>当前余额
        /// </summary>
        public double CurrentBalance { get; private set; }
        /// <summary>取款时间
        /// </summary>
        public DateTime TransactionTime { get; private set; }

        public Withdrawn(string accountId, double amount, double currentBalance, DateTime transactionTime) : base(accountId)
        {
            Amount = amount;
            CurrentBalance = currentBalance;
            TransactionTime = transactionTime;
        }
    }
}
