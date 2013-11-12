using System;
using ENode.Eventing;

namespace BankTransferSample.DomainEvents.BankAccount
{
    /// <summary>余额不足不允许取款操作
    /// </summary>
    [Serializable]
    public class WithdrawInsufficientBalance : DomainEvent<string>
    {
        /// <summary>取款金额
        /// </summary>
        public double Amount { get; private set; }
        /// <summary>当前余额
        /// </summary>
        public double CurrentBalance { get; private set; }
        /// <summary>当前可用余额
        /// </summary>
        public double CurrentAvailableBalance { get; private set; }

        public WithdrawInsufficientBalance(string accountId, double amount, double currentBalance, double currentAvailableBalance) : base(accountId)
        {
            Amount = amount;
            CurrentBalance = currentBalance;
            CurrentAvailableBalance = currentAvailableBalance;
        }
    }
}
