using System;
using ENode.Eventing;

namespace BankTransferSample.DomainEvents.BankAccount
{
    /// <summary>交易转入已提交
    /// </summary>
    [Serializable]
    public class CreditCommitted : SourcingEvent<string>
    {
        /// <summary>交易ID
        /// </summary>
        public Guid TransactionId { get; private set; }
        /// <summary>转入金额
        /// </summary>
        public double Amount { get; private set; }
        /// <summary>当前余额
        /// </summary>
        public double CurrentBalance { get; private set; }
        /// <summary>转入时间
        /// </summary>
        public DateTime TransactionTime { get; private set; }

        public CreditCommitted(string accountId, Guid transactionId, double amount, double currentBalance, DateTime transactionTime) : base(accountId)
        {
            TransactionId = transactionId;
            Amount = amount;
            CurrentBalance = currentBalance;
            TransactionTime = transactionTime;
        }
    }
}
