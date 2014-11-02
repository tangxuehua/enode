using System;
using ECommon.Utilities;
using ENode.Commanding;

namespace BankTransferSample.Commands
{
    /// <summary>发起一笔存款交易
    /// </summary>
    [Serializable]
    public class StartDepositTransactionCommand : AggregateCommand<string>, ICreatingAggregateCommand
    {
        /// <summary>账户ID
        /// </summary>
        public string AccountId { get; private set; }
        /// <summary>存款金额
        /// </summary>
        public double Amount { get; private set; }

        public StartDepositTransactionCommand(string accountId, double amount)
        {
            AccountId = accountId;
            Amount = amount;
        }
    }
    /// <summary>确认预存款
    /// </summary>
    [Serializable]
    public class ConfirmDepositPreparationCommand : AggregateCommand<string>
    {
        public ConfirmDepositPreparationCommand(string transactionId)
            : base(transactionId)
        {
        }
    }
    /// <summary>确认存款
    /// </summary>
    [Serializable]
    public class ConfirmDepositCommand : AggregateCommand<string>
    {
        public ConfirmDepositCommand(string transactionId)
            : base(transactionId)
        {
        }
    }
}
