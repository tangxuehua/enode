using System;

namespace BankTransferSample.DomainEvents.Transaction
{
    /// <summary>转账信息值对象，包含了一次转账交易的基本信息
    /// </summary>
    [Serializable]
    public class TransactionInfo
    {
        /// <summary>源账号
        /// </summary>
        public string SourceAccountId { get; private set; }
        /// <summary>目标账号
        /// </summary>
        public string TargetAccountId { get; private set; }
        /// <summary>转账金额
        /// </summary>
        public double Amount { get; private set; }

        public TransactionInfo(string sourceAccountId, string targetAccountId, double amount)
        {
            SourceAccountId = sourceAccountId;
            TargetAccountId = targetAccountId;
            Amount = amount;
        }
    }
}
