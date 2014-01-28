using System;
using ENode.Eventing;
using Newtonsoft.Json;

namespace BankTransferSample.DomainEvents.BankAccount
{
    /// <summary>已存款
    /// </summary>
    [Serializable]
    public class Deposited : SourcingEvent<string>
    {
        /// <summary>存款金额
        /// </summary>
        public double Amount { get; private set; }
        /// <summary>当前余额
        /// </summary>
        public double CurrentBalance { get; private set; }
        /// <summary>存款时间
        /// </summary>
        public DateTime TransactionTime { get; private set; }

        public Deposited(string accountId, double amount, double currentBalance, DateTime transactionTime) : base(accountId)
        {
            Amount = amount;
            CurrentBalance = currentBalance;
            TransactionTime = transactionTime;
        }
    }
}
