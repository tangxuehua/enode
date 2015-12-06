using System;
using ENode.Commanding;
using ENode.Infrastructure;

namespace BankTransferSample.Commands
{
    /// <summary>发起一笔存款交易
    /// </summary>
    public class StartDepositTransactionCommand : Command
    {
        /// <summary>账户ID
        /// </summary>
        public string AccountId { get; set; }
        /// <summary>存款金额
        /// </summary>
        public double Amount { get; set; }

        public StartDepositTransactionCommand() { }
        public StartDepositTransactionCommand(string transactionId, string accountId, double amount)
            : base(transactionId)
        {
            AccountId = accountId;
            Amount = amount;
        }
    }
    /// <summary>确认预存款
    /// </summary>
    public class ConfirmDepositPreparationCommand : Command
    {
        public ConfirmDepositPreparationCommand() { }
        public ConfirmDepositPreparationCommand(string transactionId)
            : base(transactionId)
        {
        }
    }
    /// <summary>确认存款
    /// </summary>
    public class ConfirmDepositCommand : Command
    {
        public ConfirmDepositCommand() { }
        public ConfirmDepositCommand(string transactionId)
            : base(transactionId)
        {
        }
    }
}
