using System;
using ENode.Eventing;
using Newtonsoft.Json;

namespace BankTransferSample.DomainEvents.BankAccount
{
    /// <summary>余额不足不允许转出操作
    /// </summary>
    [Serializable]
    public class DebitInsufficientBalance : DomainEvent<string>
    {
        /// <summary>交易ID
        /// </summary>
        public Guid TransactionId { get; private set; }
        /// <summary>转出金额
        /// </summary>
        public double Amount { get; private set; }
        /// <summary>当前余额
        /// </summary>
        public double CurrentBalance { get; private set; }
        /// <summary>当前可用余额
        /// </summary>
        public double CurrentAvailableBalance { get; private set; }

        public DebitInsufficientBalance(string accountId, Guid transactionId, double amount, double currentBalance, double currentAvailableBalance) : base(accountId)
        {
            TransactionId = transactionId;
            Amount = amount;
            CurrentBalance = currentBalance;
            CurrentAvailableBalance = currentAvailableBalance;
        }
    }
}
